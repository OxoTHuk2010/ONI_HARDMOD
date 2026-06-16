# Hardcore Systems

## Purpose

Hardcore Systems is a modular Oxygen Not Included difficulty overhaul. Version 0.4.3 includes the foundation, duplicant/mining gameplay modules, building heat scaling, Solar Panel generation heat, emergency wire/bridge overload heating, and optional runtime diagnostics.

## Architecture

- `ModEntry` is the ONI/KMod entry point and delegates startup to `ModBootstrap`.
- `ModContext` carries stable runtime state: mod identity, paths, settings, DLC data, and logger.
- `ModuleRegistry` owns gameplay modules and isolates module failures.
- `MiningYield`, `DuplicantBalance`, `DiseaseEffects`, `SolarGeneration`, `IndustrialHeat`, and `ElectricalOverloadThermalDamage` are independent gameplay modules. If one patch target is missing, the module logs the failure and the rest of the mod continues loading.
- `DiagnosticsRuntime` records optional lightweight module metrics only when diagnostics are enabled.
- `Configuration` contains DTOs, preset generation, schema versioning, and validation.
- `Diagnostics` contains structured and rate-limited logging.
- `Bootstrap` contains DLC detection, compatibility capture, localization registration, and safe patch helpers.
- `Persistence` stores global config under the local mod folder.

## Requirements

- Oxygen Not Included installed at `D:\SteamLibrary\steamapps\common\OxygenNotIncluded`, or pass `-OniGameDir` to scripts.
- Visual Studio/MSBuild capable of building C# projects.
- .NET Framework 4.8 reference assemblies.
- Game managed assemblies from `OxygenNotIncluded_Data\Managed`.
- No NuGet restore is required for this stage.

## Configuration

The default preset is `Off`. All gameplay modules are disabled by default. The config schema version is stored in `config\hardcore_systems.json`.

Supported presets:

- `Off`
- `VanillaPlus`
- `Hard`
- `Extreme`
- `Insane`
- `Custom`

Validation rejects NaN, infinity, negative values where unsafe, and values outside documented ranges.

Until the options UI is implemented, enable gameplay by editing `config\hardcore_systems.json` in the installed mod folder. For v0.4.3 testing, enable `Mining`, `Duplicants`, `Diseases`, `Power.SolarPanelGenerationHeatEnabled`, `Power.OverloadHeatEnabled`, `IndustrialHeat.Enabled`, and optionally `Diagnostics.Enabled`, then restart ONI. `Power.GeneratorEfficiencyEnabled`, `Power.GeneratorEfficiency`, `Power.ElectricalLossesEnabled`, `Power.TransformerEfficiencyEnabled`, and `Power.WireResistanceMultiplier` are retained for config compatibility but are not used by v0.4.3 gameplay.

## Usage

Build:

```powershell
.\tools\build.ps1 -Configuration Release -OniGameDir "D:\SteamLibrary\steamapps\common\OxygenNotIncluded"
```

If MSBuild fails because of the local Visual Studio compiler setup, the script falls back to direct compiler invocation. When Roslyn is available, fallback compilation still uses the real ONI reference assemblies; legacy compile-time stubs are used only when Roslyn is unavailable. These stubs are not installed with the mod.

Install locally:

```powershell
.\tools\install-local.ps1 -Configuration Release
```

Verify load:

1. Start Oxygen Not Included.
2. Enable `Hardcore Systems` in the Mods menu.
3. Restart the game when ONI asks.
4. Check `Player.log` for `Hardcore Systems loaded`.

## Safety Model

Gameplay patches are registered through `PatchGuard`. Mining yield patches `WorldDamage.OnDigComplete`; calorie balance uses a runtime `Calories.deltaAttribute` modifier; building heat scaling patches `StructureTemperaturePayload.OperatingKilowatts`, `StructureTemperaturePayload.ExhaustKilowatts`, and `Building.EffectDescriptors` for any building with positive heat output. Solar heat patches `SolarPanel.EnergySim200ms(float)` and adds heat from `SolarPanel.CurrentWattage` through ONI's building thermal simulation. Electrical overload heating patches `BuildingHP.DoDamage`; it only handles overload damage metadata, checks `Wire`/`WireUtilityNetworkLink`/wire bridge interfaces, calculates energy from material melting point, mass, and SHC, then injects energy through `SimMessages.ModifyBuildingEnergy` when the building thermal sim handle is available. The mod does not patch saves directly.

## Testing

Run the included smoke tests through MSBuild:

```powershell
.\tools\build.ps1 -Configuration Release
.\src\HardcoreSystems.Tests\bin\Release\HardcoreSystems.Tests.exe
```

## Deployment

The local install script copies the compiled DLL, manifest files, and localization files into:

```text
%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\ONI.HardcoreSystems
```

Workshop packaging is not automated in this stage.

## Troubleshooting

- If MSBuild is missing, install Visual Studio Build Tools with the C# workload.
- If MSBuild reports compiler setup errors, `tools\build.ps1` should still produce the DLL through fallback compilation. Installing or repairing the Visual Studio C# workload may remove that warning path.
- If references fail, pass the correct `-OniGameDir`.
- If the mod does not appear, verify that `mod.yaml`, `mod_info.yaml`, and `HardcoreSystems.Mod.dll` exist in the local mod folder.
- If ONI logs reference load errors, confirm the game build still ships `0Harmony.dll`, `Assembly-CSharp.dll`, `Assembly-CSharp-firstpass.dll`, `UnityEngine.CoreModule.dll`, and `Newtonsoft.Json.dll`.
- Generator output reduction and generator thermal rebalance are intentionally inactive in v0.4.3; generator wattage, fuel consumption, and product masses should remain vanilla unless another mod changes them.
- If building heat scaling is not visible, test with a Coal Generator, Research Station, Liquid Pump, or Gas Pump, confirm the tooltip heat value changed after restart, and increase `IndustrialHeat.HeatMultiplier` conservatively.
- If Solar Panel heat is not visible, confirm `Power.SolarPanelGenerationHeatEnabled=true`; test at different light levels and confirm generated heat follows actual wattage.
- If wire emergency heat is not visible, confirm `Power.OverloadHeatEnabled=true` and test with actual overload damage on a regular wire or a wire bridge.
- Wire resistance, length-based losses, voltage-drop, consumer brownouts, generator output reduction, and transformer efficiency are intentionally inactive in v0.4.3.
