param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [string]$OniGameDir = "D:\SteamLibrary\steamapps\common\OxygenNotIncluded"
)

$ErrorActionPreference = "Stop"
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$solution = Join-Path $repoRoot "ONI.HardcoreSystems.sln"
$managed = Join-Path $OniGameDir "OxygenNotIncluded_Data\Managed"

if (-not (Test-Path $managed)) {
    throw "ONI managed directory not found: $managed"
}

$msbuildCandidates = @(
    "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
)

$msbuild = $msbuildCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $msbuild) {
    $fromPath = Get-Command MSBuild.exe -ErrorAction SilentlyContinue
    if ($fromPath) {
        $msbuild = $fromPath.Source
    }
}

if (-not $msbuild) {
    throw "MSBuild.exe was not found. Install Visual Studio Build Tools with the C# workload."
}

& $msbuild $solution /m /restore:false /p:Configuration=$Configuration /p:OniGameDir="$OniGameDir" /p:UseSharedCompilation=false
if ($LASTEXITCODE -eq 0) {
    return
}

$roslynTargets = Join-Path (Split-Path $msbuild -Parent) "Roslyn\Microsoft.CSharp.Core.targets"
$roslynCsc = Join-Path (Split-Path $msbuild -Parent) "Roslyn\csc.exe"

Write-Warning "MSBuild failed with exit code $LASTEXITCODE; falling back to direct compiler invocation."
$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $csc)) {
    $csc = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
}

if (Test-Path $roslynCsc) {
    $csc = $roslynCsc
} elseif (-not (Test-Path $csc)) {
    throw "No fallback C# compiler was found. Install Visual Studio Build Tools with the C# workload."
}

$modOut = Join-Path $repoRoot "src\HardcoreSystems.Mod\bin\$Configuration"
$testOut = Join-Path $repoRoot "src\HardcoreSystems.Tests\bin\$Configuration"
$fallbackRefs = Join-Path $repoRoot "obj\fallback_refs"
New-Item -ItemType Directory -Force -Path $modOut | Out-Null
New-Item -ItemType Directory -Force -Path $testOut | Out-Null
$modSources = Get-ChildItem -Recurse -File (Join-Path $repoRoot "src\HardcoreSystems.Mod") -Filter "*.cs" |
    Where-Object { $_.FullName -notmatch "\\obj\\|\\bin\\" } |
    ForEach-Object { $_.FullName }

if (Test-Path $roslynCsc) {
    $frameworkRefRoot = "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8"
    $modRefPaths = @(
        (Join-Path $frameworkRefRoot "mscorlib.dll"),
        (Join-Path $frameworkRefRoot "System.dll"),
        (Join-Path $frameworkRefRoot "System.Core.dll"),
        (Join-Path $managed "0Harmony.dll"),
        (Join-Path $managed "Assembly-CSharp.dll"),
        (Join-Path $managed "Assembly-CSharp-firstpass.dll"),
        (Join-Path $managed "netstandard.dll"),
        (Join-Path $managed "Newtonsoft.Json.dll"),
        (Join-Path $managed "UnityEngine.dll"),
        (Join-Path $managed "UnityEngine.CoreModule.dll"),
        (Join-Path $managed "UnityEngine.UI.dll")
    )
    $modRefs = $modRefPaths | ForEach-Object { "/reference:" + $_ }
    $modArgs = @("/noconfig", "/nostdlib+", "/target:library", "/langversion:7.3", "/optimize+", "/out:$(Join-Path $modOut 'HardcoreSystems.Mod.dll')") + $modRefs + $modSources
} else {
    New-Item -ItemType Directory -Force -Path $fallbackRefs | Out-Null

    & $csc /nologo /target:library "/out:$(Join-Path $fallbackRefs '0Harmony.dll')" (Join-Path $repoRoot "tools\stubs\0Harmony\HarmonyStubs.cs")
    if ($LASTEXITCODE -ne 0) {
        throw "Fallback Harmony reference stub compilation failed with exit code $LASTEXITCODE"
    }

    & $csc /nologo /target:library "/out:$(Join-Path $fallbackRefs 'Assembly-CSharp.dll')" "/reference:$(Join-Path $fallbackRefs '0Harmony.dll')" (Join-Path $repoRoot "tools\stubs\Assembly-CSharp\KModStubs.cs")
    if ($LASTEXITCODE -ne 0) {
        throw "Fallback KMod reference stub compilation failed with exit code $LASTEXITCODE"
    }

    & $csc /nologo /target:library "/out:$(Join-Path $fallbackRefs 'UnityEngine.CoreModule.dll')" (Join-Path $repoRoot "tools\stubs\UnityEngine.CoreModule\UnityStubs.cs")
    if ($LASTEXITCODE -ne 0) {
        throw "Fallback Unity reference stub compilation failed with exit code $LASTEXITCODE"
    }

    $modRefs = @(
        "/reference:$(Join-Path $fallbackRefs '0Harmony.dll')",
        "/reference:$(Join-Path $fallbackRefs 'Assembly-CSharp.dll')",
        "/reference:$(Join-Path $fallbackRefs 'UnityEngine.CoreModule.dll')",
        "/reference:$(Join-Path $managed 'Newtonsoft.Json.dll')"
    )
    $modArgs = @("/nologo", "/target:library", "/optimize+", "/out:$(Join-Path $modOut 'HardcoreSystems.Mod.dll')") + $modRefs + $modSources
}

& $csc @modArgs
if ($LASTEXITCODE -ne 0) {
    throw "Fallback mod compilation failed with exit code $LASTEXITCODE"
}

$testSources = Get-ChildItem -Recurse -File (Join-Path $repoRoot "src\HardcoreSystems.Tests") -Filter "*.cs" |
    Where-Object { $_.FullName -notmatch "\\obj\\|\\bin\\" } |
    ForEach-Object { $_.FullName }
$testRefs = $modRefs + @("/reference:$(Join-Path $modOut 'HardcoreSystems.Mod.dll')")
if (Test-Path $roslynCsc) {
    $testArgs = @("/noconfig", "/nostdlib+", "/target:exe", "/langversion:7.3", "/optimize+", "/out:$(Join-Path $testOut 'HardcoreSystems.Tests.exe')") + $testRefs + $testSources
} else {
    $testArgs = @("/nologo", "/target:exe", "/optimize+", "/out:$(Join-Path $testOut 'HardcoreSystems.Tests.exe')") + $testRefs + $testSources
}
& $csc @testArgs
if ($LASTEXITCODE -ne 0) {
    throw "Fallback test compilation failed with exit code $LASTEXITCODE"
}

Copy-Item -Force (Join-Path $modOut "HardcoreSystems.Mod.dll") $testOut
if (Test-Path $fallbackRefs) {
    Copy-Item -Force (Join-Path $fallbackRefs "*.dll") $testOut -ErrorAction SilentlyContinue
}
