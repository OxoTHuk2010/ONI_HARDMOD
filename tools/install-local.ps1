param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [string]$OniGameDir = "D:\SteamLibrary\steamapps\common\OxygenNotIncluded",
    [string]$Destination = "$env:USERPROFILE\Documents\Klei\OxygenNotIncluded\mods\Local\ONI.HardcoreSystems",
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")

if (-not $SkipBuild) {
    & (Join-Path $PSScriptRoot "build.ps1") -Configuration $Configuration -OniGameDir $OniGameDir
}

$output = Join-Path $repoRoot "src\HardcoreSystems.Mod\bin\$Configuration"
$dll = Join-Path $output "HardcoreSystems.Mod.dll"
if (-not (Test-Path $dll)) {
    throw "Compiled mod DLL not found: $dll"
}

function Copy-CleanDirectory($Source, $Target) {
    if (-not (Test-Path $Source)) {
        return
    }

    $destinationRoot = [System.IO.Path]::GetFullPath($Destination)
    $targetPath = [System.IO.Path]::GetFullPath($Target)
    if (-not $targetPath.StartsWith($destinationRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to clean directory outside destination: $targetPath"
    }

    if (Test-Path $targetPath) {
        Remove-Item -LiteralPath $targetPath -Recurse -Force
    }

    Copy-Item -Force -Recurse $Source $Target
}

New-Item -ItemType Directory -Force -Path $Destination | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $Destination "Localization") | Out-Null

Copy-Item -Force $dll $Destination
Copy-Item -Force (Join-Path $repoRoot "mod.yaml") $Destination
Copy-Item -Force (Join-Path $repoRoot "mod_info.yaml") $Destination
Copy-Item -Force (Join-Path $repoRoot "src\HardcoreSystems.Mod\Localization\*.po") (Join-Path $Destination "Localization")

$worldgen = Join-Path $repoRoot "src\HardcoreSystems.Mod\worldgen"
Copy-CleanDirectory $worldgen (Join-Path $Destination "worldgen")

$dlc = Join-Path $repoRoot "src\HardcoreSystems.Mod\dlc"
Copy-CleanDirectory $dlc (Join-Path $Destination "dlc")

Write-Host "Installed Hardcore Systems to $Destination"
