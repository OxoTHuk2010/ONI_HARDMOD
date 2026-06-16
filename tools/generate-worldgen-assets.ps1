param(
    [string]$OniGameDir = "D:\SteamLibrary\steamapps\common\OxygenNotIncluded"
)

$ErrorActionPreference = "Stop"
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$assets = Join-Path $OniGameDir "OxygenNotIncluded_Data\StreamingAssets"
if (-not (Test-Path $assets)) {
    throw "ONI StreamingAssets directory not found: $assets"
}

$outRoot = Join-Path $repoRoot "src\HardcoreSystems.Mod"
$generatedRoots = @(
    (Join-Path $outRoot "worldgen"),
    (Join-Path $outRoot "dlc\expansion1")
)
foreach ($generatedRoot in $generatedRoots) {
    $resolvedOutRoot = [System.IO.Path]::GetFullPath($outRoot)
    $resolvedGeneratedRoot = [System.IO.Path]::GetFullPath($generatedRoot)
    if ($resolvedGeneratedRoot.StartsWith($resolvedOutRoot) -and (Test-Path $resolvedGeneratedRoot)) {
        Remove-Item -LiteralPath $resolvedGeneratedRoot -Recurse -Force
    }
}

$variantSpecs = @(
    @{ Name = "Half"; AreaScale = 0.50; AxisScale = [Math]::Sqrt(0.50); Prefix = "HCS-H"; RemoveWorldTemplateRules = $false; CompactSubworlds = $false },
    @{ Name = "Quarter"; AreaScale = 0.25; AxisScale = 0.50; Prefix = "HCS-Q"; RemoveWorldTemplateRules = $true; CompactSubworlds = $true }
)

function Read-Lines($path) {
    return [System.Collections.Generic.List[string]](Get-Content -LiteralPath $path)
}

function Write-Lines($path, $lines) {
    $dir = Split-Path $path -Parent
    New-Item -ItemType Directory -Force -Path $dir | Out-Null
    [System.IO.File]::WriteAllLines($path, [string[]]$lines)
}

function Scale-Even([int]$value, [double]$scale, [int]$minimum) {
    $scaled = [Math]::Max($minimum, [Math]::Round($value * $scale))
    return [int]($scaled - ($scaled % 2))
}

function Is-CountLine([string]$line) {
    return $line -match '^\s*(minCount|maxCount):\s*\d+\s*$'
}

function Is-TopLevelKey([string]$line) {
    return $line -match '^[A-Za-z0-9_]+:'
}

function Normalize-GeneratedWorldRules($lines, [bool]$removeWorldTemplateRules) {
    $result = New-Object 'System.Collections.Generic.List[string]'
    $inSubworldFiles = $false
    $inMixingRules = $false
    $skippingBlock = $false
    $hasDisableWorldTraits = $false

    foreach ($line in $lines) {
        if (Is-TopLevelKey $line) {
            $skippingBlock = $line -match '^(worldTraitRules|subworldMixingRules):'
            if ($removeWorldTemplateRules -and $line -match '^worldTemplateRules:') {
                $skippingBlock = $true
            }

            $inSubworldFiles = $line -match '^subworldFiles:'
            $inMixingRules = $line -match '^subworldMixingRules:'
            if ($line -match '^disableWorldTraits:') {
                $hasDisableWorldTraits = $true
                $result.Add('disableWorldTraits: true')
                continue
            }
        }

        if ($skippingBlock) {
            continue
        }

        if ($removeWorldTemplateRules -and $line -match '^\s*-\s*\(Mixing[0-9]+\)\s*$') {
            continue
        }

        if ($inSubworldFiles -and (Is-CountLine $line)) {
            continue
        }

        if ($removeWorldTemplateRules -and $inSubworldFiles -and $line -match '^\s*overridePower:\s*') {
            continue
        }

        $result.Add($line)
    }

    if (-not $hasDisableWorldTraits) {
        $insertAt = 0
        for ($i = 0; $i -lt $result.Count; $i++) {
            if ($result[$i] -match '^description:') {
                $insertAt = $i + 1
                break
            }
        }

        $result.Insert($insertAt, 'disableWorldTraits: true')
    }

    return ,$result
}

function Normalize-GeneratedSubworldRules($lines) {
    $result = New-Object 'System.Collections.Generic.List[string]'
    $skippingBlock = $false

    foreach ($line in $lines) {
        if (Is-TopLevelKey $line) {
            $skippingBlock = $line -match '^(features|featureTemplates|templateRules|subworldTemplateRules):'
        }

        if ($skippingBlock) {
            continue
        }

        if ($line -match '^avoidRadius:\s*') {
            $result.Add('avoidRadius: 3')
            continue
        }

        if ($line -match '^pdWeight:\s*') {
            $result.Add('pdWeight: 0.5')
            continue
        }

        if ($line -match '^minChildCount:\s*') {
            $result.Add('minChildCount: 1')
            continue
        }

        if ($line -match '^dontRelaxChildren:\s*') {
            $result.Add('dontRelaxChildren: false')
            continue
        }

        $result.Add($line)
    }

    return ,$result
}

function Remove-TopLevelBlock($lines, [string]$blockName) {
    $result = New-Object 'System.Collections.Generic.List[string]'
    $skippingBlock = $false
    $pattern = '^' + [Regex]::Escape($blockName) + ':'

    foreach ($line in $lines) {
        if (Is-TopLevelKey $line) {
            $skippingBlock = $line -match $pattern
        }

        if (-not $skippingBlock) {
            $result.Add($line)
        }
    }

    return ,$result
}

function Add-UniqueRef($list, [string]$ref) {
    if ($ref -and -not $list.Contains($ref)) {
        $list.Add($ref)
    }
}

function Select-Refs($refs, [string[]]$includePatterns, [string[]]$excludePatterns) {
    $selected = New-Object 'System.Collections.Generic.List[string]'

    foreach ($ref in $refs) {
        $lower = $ref.ToLowerInvariant()
        $included = $false
        foreach ($pattern in $includePatterns) {
            if ($lower -match $pattern) {
                $included = $true
                break
            }
        }

        if (-not $included) {
            continue
        }

        $excluded = $false
        foreach ($pattern in $excludePatterns) {
            if ($lower -match $pattern) {
                $excluded = $true
                break
            }
        }

        if (-not $excluded) {
            Add-UniqueRef $selected $ref
        }
    }

    return $selected
}

function Get-WorldSubworldRefs($lines) {
    $refs = New-Object 'System.Collections.Generic.List[string]'
    $startRef = $null
    $inSubworldFiles = $false

    foreach ($line in $lines) {
        if (Is-TopLevelKey $line) {
            $inSubworldFiles = $line -match '^subworldFiles:'
        }

        if ($line -match '^startSubworldName:\s*([^#\s]+)') {
            $startRef = $matches[1]
            Add-UniqueRef $refs $startRef
            continue
        }

        if ($inSubworldFiles -and $line -match '^\s*-\s*name:\s*([^#\s]+)') {
            Add-UniqueRef $refs $matches[1]
        }
    }

    return @{ Start = $startRef; Refs = $refs }
}

function Add-SubworldNamesBlock($lines, $refs) {
    foreach ($ref in $refs) {
        $lines.Add("      - $ref")
    }
}

function Rewrite-QuarterUnknownCellsAllowedSubworlds($lines) {
    $metadata = Get-WorldSubworldRefs $lines
    $refs = $metadata.Refs
    if ($refs.Count -eq 0) {
        return $lines
    }

    $startRef = $metadata.Start
    $surface = Select-Refs $refs @('(^|/)space/', 'surface', 'regolith') @()
    $magma = Select-Refs $refs @('(^|/)magma/', '(^|/)bottom$', '/bottom$') @()
    $water = Select-Refs $refs @('water', 'ocean', 'slush', 'ice', 'frozen') @('(^|/)space/', 'surface')
    $nearStart = Select-Refs $refs @('mini', 'water', 'sandstone', 'forest', 'barren', 'granite', 'frozen') @('(^|/)space/', 'surface', '(^|/)magma/', '(^|/)bottom$', '/bottom$')
    $mid = New-Object 'System.Collections.Generic.List[string]'
    $filteredNearStart = New-Object 'System.Collections.Generic.List[string]'

    foreach ($ref in $refs) {
        $lower = $ref.ToLowerInvariant()
        if ($ref -eq $startRef) {
            continue
        }

        if ($lower -match '\(mixing[0-9]+\)') {
            continue
        }

        if ($lower -match '(^|/)space/' -or $lower -match 'surface' -or $lower -match 'regolith') {
            continue
        }

        if ($lower -match '(^|/)magma/' -or $lower -match '(^|/)bottom$' -or $lower -match '/bottom$') {
            continue
        }

        Add-UniqueRef $mid $ref
    }

    foreach ($ref in $nearStart) {
        if ($ref -ne $startRef) {
            Add-UniqueRef $filteredNearStart $ref
        }
    }
    $nearStart = $filteredNearStart

    if ($nearStart.Count -eq 0) {
        $nearStart = $mid
    }

    $lines = Remove-TopLevelBlock $lines 'unknownCellsAllowedSubworlds'
    $lines.Add('')
    $lines.Add('unknownCellsAllowedSubworlds:')
    $lines.Add('  # Hardcore Systems Quarter compact fill. Keep every biome pocket small and mixed.')
    $lines.Add('  - tagcommand: Default')
    $lines.Add('    command: Replace')
    $lines.Add('    subworldNames:')
    Add-SubworldNamesBlock $lines $mid

    $lines.Add('  # Compact ring around the printing pod.')
    $lines.Add('  - tagcommand: DistanceFromTag')
    $lines.Add('    tag: AtStart')
    $lines.Add('    minDistance: 1')
    $lines.Add('    maxDistance: 1')
    $lines.Add('    command: Replace')
    $lines.Add('    subworldNames:')
    Add-SubworldNamesBlock $lines $nearStart

    if ($water.Count -gt 0) {
        $lines.Add('  # Keep at least water-capable biomes available near the start without reserving a huge region.')
        $lines.Add('  - tagcommand: DistanceFromTag')
        $lines.Add('    tag: AtStart')
        $lines.Add('    minDistance: 1')
        $lines.Add('    maxDistance: 2')
        $lines.Add('    command: UnionWith')
        $lines.Add('    subworldNames:')
        Add-SubworldNamesBlock $lines $water
    }

    if ($mid.Count -gt 0) {
        $lines.Add('  # Mix all non-surface/non-magma biomes after the starter pocket.')
        $lines.Add('  - tagcommand: DistanceFromTag')
        $lines.Add('    tag: AtStart')
        $lines.Add('    minDistance: 2')
        $lines.Add('    maxDistance: 99')
        $lines.Add('    command: UnionWith')
        $lines.Add('    subworldNames:')
        Add-SubworldNamesBlock $lines $mid
    }

    if ($magma.Count -gt 0) {
        $lines.Add('  # Quarter keeps lava/core content to the bottom band only.')
        $lines.Add('  - tagcommand: AtTag')
        $lines.Add('    tag: AtDepths')
        $lines.Add('    command: Replace')
        $lines.Add('    subworldNames:')
        Add-SubworldNamesBlock $lines $magma
    }

    if ($surface.Count -gt 0) {
        $lines.Add('  # Quarter keeps space/regolith content to the surface band only.')
        $lines.Add('  - tagcommand: AtTag')
        $lines.Add('    tag: AtSurface')
        $lines.Add('    command: Replace')
        $lines.Add('    subworldNames:')
        Add-SubworldNamesBlock $lines $surface
    }

    return $lines
}

function Add-QuarterWaterTemplateRules($lines, [bool]$isDlcWorld) {
    $lines = Remove-TopLevelBlock $lines 'worldTemplateRules'
    $lines.Add('')
    $lines.Add('worldTemplateRules:')
    $lines.Add('  # One optional water-focused geyser/vent for Quarter. TryOne avoids hard generation failures on tight maps.')
    $lines.Add('  - names:')
    $lines.Add('      - geysers/steam')
    $lines.Add('      - geysers/salt_water')
    $lines.Add('      - geysers/filthy_water')
    $lines.Add('      - poi/poi_ocean_geyser_saltwater')
    $lines.Add('      - poi/jungle/geyser_steam')
    $lines.Add('      - poi/hotmarsh/geyser_steam')
    if ($isDlcWorld) {
        $lines.Add('      - expansion1::geysers/slush_salt_water')
    }
    $lines.Add('    listRule: TryOne')
    $lines.Add('    priority: 25')
    $lines.Add('    useRelaxedFiltering: true')
    $lines.Add('    allowedCellsFilter:')
    $lines.Add('      - command: Replace')
    $lines.Add('        tagcommand: DistanceFromTag')
    $lines.Add('        tag: AtStart')
    $lines.Add('        minDistance: 1')
    $lines.Add('        maxDistance: 99')
    $lines.Add('      - command: ExceptWith')
    $lines.Add('        zoneTypes: [ Space, MagmaCore ]')

    return $lines
}

function Create-CompactSubworld($subworldRef, $spec) {
    if (-not [bool]$spec.CompactSubworlds) {
        return $subworldRef
    }

    $refPrefix = ""
    $relative = $subworldRef
    $sourceRoot = $assets
    if ($subworldRef -match '^([^:]+)::(.+)$') {
        $refPrefix = $matches[1]
        $relative = $matches[2]
        $sourceRoot = Join-Path $assets "dlc\$refPrefix"
    }

    if ($relative -notmatch '^subworlds/') {
        return $subworldRef
    }

    $source = Join-Path $sourceRoot ("worldgen\" + ($relative -replace '/', '\') + ".yaml")
    if (-not (Test-Path $source)) {
        return $subworldRef
    }

    $directory = (Split-Path $relative -Parent) -replace '\\','/'
    $name = Split-Path $relative -Leaf
    $targetName = "HardcoreSystems_$($spec.Name)_$name"
    $targetRelative = if ($directory) { ($directory + "/" + $targetName) } else { $targetName }
    $targetRoot = if ($refPrefix -eq "") { Join-Path $outRoot "worldgen" } else { Join-Path $outRoot "dlc\$refPrefix\worldgen" }
    $target = Join-Path $targetRoot (($targetRelative -replace '/', '\') + ".yaml")

    if (-not (Test-Path $target)) {
        $lines = Normalize-GeneratedSubworldRules (Read-Lines $source)
        Write-Lines $target $lines
    }

    if ($refPrefix -eq "") {
        return $targetRelative
    }

    return "$refPrefix`::$targetRelative"
}

function Create-World($sourceRoot, $prefix, $worldName, $spec) {
    $source = Join-Path $sourceRoot "worldgen\worlds\$worldName.yaml"
    if (-not (Test-Path $source)) {
        return $null
    }

    $targetName = "HardcoreSystems_$($spec.Name)_$worldName"
    $targetRoot = if ($prefix -eq "") { Join-Path $outRoot "worldgen\worlds" } else { Join-Path $outRoot "dlc\$prefix\worldgen\worlds" }
    $target = Join-Path $targetRoot "$targetName.yaml"
    $lines = Normalize-GeneratedWorldRules (Read-Lines $source) ([bool]$spec.RemoveWorldTemplateRules)

    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match '^name:') {
            $lines[$i] = "name: Hardcore $worldName $($spec.Name)"
        } elseif ($lines[$i] -match '^description:') {
            $lines[$i] = "description: Hardcore Systems $($spec.Name) asteroid-size variant generated from $worldName."
        } elseif ($lines[$i] -match '^worldTraitScale:\s*([0-9.]+)') {
            $value = [double]$matches[1]
            $lines[$i] = "worldTraitScale: " + ($value * $spec.AreaScale).ToString("0.###", [System.Globalization.CultureInfo]::InvariantCulture) + " # Hardcore Systems v0.7 area-scaled"
        } elseif ($lines[$i] -match '^\s*X:\s*(\d+)\s*$') {
            $lines[$i] = "  X: " + (Scale-Even ([int]$matches[1]) $spec.AxisScale 96)
        } elseif ($lines[$i] -match '^\s*Y:\s*(\d+)\s*$') {
            $lines[$i] = "  Y: " + (Scale-Even ([int]$matches[1]) $spec.AxisScale 128)
        } elseif ($lines[$i] -match '^(\s*-\s*name:\s*)([A-Za-z0-9_:\/]+)(.*)$') {
            $lines[$i] = $matches[1] + (Create-CompactSubworld $matches[2] $spec) + $matches[3]
        } elseif ($lines[$i] -match '^(startSubworldName:\s*)([A-Za-z0-9_:\/]+)(.*)$') {
            $lines[$i] = $matches[1] + (Create-CompactSubworld $matches[2] $spec) + $matches[3]
        } elseif ($lines[$i] -match '^(\s*-\s*)((?:[A-Za-z0-9_]+::)?subworlds/[A-Za-z0-9_/]+)(.*)$') {
            $lines[$i] = $matches[1] + (Create-CompactSubworld $matches[2] $spec) + $matches[3]
        }
    }

    if ([bool]$spec.CompactSubworlds) {
        $lines = Rewrite-QuarterUnknownCellsAllowedSubworlds $lines
        $lines = Add-QuarterWaterTemplateRules $lines ($prefix -ne "")
    }

    Write-Lines $target $lines
    return $targetName
}

function Is-StartWorldPlacement($lines, [int]$worldLineIndex) {
    for ($i = $worldLineIndex + 1; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match '^\s*-\s*world:') {
            return $false
        }

        if ($lines[$i] -match '^\s*locationType:\s*StartWorld\s*$') {
            return $true
        }
    }

    return $false
}

function Create-Cluster($sourceRoot, $prefix, $clusterName, $allowedCategory, $spec, [bool]$startWorldOnly) {
    $source = Join-Path $sourceRoot "worldgen\clusters\$clusterName.yaml"
    $lines = Read-Lines $source
    $category = ($lines | Select-String '^clusterCategory:' | Select-Object -First 1).Line -replace 'clusterCategory:\s*',''
    $skip = ($lines | Select-String '^skip:' | Select-Object -First 1).Line
    if ($category -ne $allowedCategory -or $skip) {
        return
    }

    $targetName = "HardcoreSystems_$($spec.Name)_$clusterName"
    $targetRoot = if ($prefix -eq "") { Join-Path $outRoot "worldgen\clusters" } else { Join-Path $outRoot "dlc\$prefix\worldgen\clusters" }
    $target = Join-Path $targetRoot "$targetName.yaml"

    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match '^name:') {
            $lines[$i] = "name: Hardcore $clusterName $($spec.Name)"
        } elseif ($lines[$i] -match '^description:') {
            $lines[$i] = "description: Separate Hardcore Systems $($spec.Name) asteroid-size preset. New worlds only."
        } elseif ($lines[$i] -match '^coordinatePrefix:\s*(.+)\s*$') {
            $lines[$i] = "coordinatePrefix: $($spec.Prefix)-" + $matches[1]
        } elseif ($lines[$i] -match '^menuOrder:\s*(\d+)\s*$') {
            $lines[$i] = "menuOrder: " + ([int]$matches[1] + 100)
        } elseif ($lines[$i] -match '^(\s*-\s*world:\s*)(worlds|expansion1::worlds)/([A-Za-z0-9_]+)(.*)$') {
            if ($startWorldOnly -and -not (Is-StartWorldPlacement $lines $i)) {
                continue
            }

            $worldName = $matches[3]
            $newWorld = Create-World $sourceRoot $prefix $worldName $spec
            if ($newWorld) {
                $referencePrefix = if ($prefix -eq "") { "worlds" } else { "$prefix::worlds" }
                $lines[$i] = "$($matches[1])$referencePrefix/$newWorld$($matches[4])"
            }
        }
    }

    Write-Lines $target $lines
}

$baseRoot = $assets
foreach ($spec in $variantSpecs) {
    foreach ($cluster in Get-ChildItem -Path (Join-Path $baseRoot "worldgen\clusters") -Filter *.yaml) {
        Create-Cluster $baseRoot "" $cluster.BaseName "Vanilla" $spec $false
    }
}

$expansionRoot = Join-Path $assets "dlc\expansion1"
if (Test-Path $expansionRoot) {
    foreach ($spec in $variantSpecs) {
        foreach ($cluster in Get-ChildItem -Path (Join-Path $expansionRoot "worldgen\clusters") -Filter "Vanilla*.yaml") {
            Create-Cluster $expansionRoot "expansion1" $cluster.BaseName "SpacedOutVanillaStyle" $spec $true
        }
    }
}

Write-Host "Generated Hardcore Systems worldgen assets under $outRoot"
