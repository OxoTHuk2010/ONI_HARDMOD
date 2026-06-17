# Hardcore Systems

## Purpose

Hardcore Systems is a modular Oxygen Not Included difficulty overhaul. Version 0.7.7 includes the foundation, duplicant/mining gameplay modules, generator rebalance profiles, building heat scaling for non-generators, Solar Panel generation heat, emergency wire/bridge overload heating, experimental asteroid-size worldgen presets, and optional runtime diagnostics.

## Architecture

- `ModEntry` is the ONI/KMod entry point and delegates startup to `ModBootstrap`.
- `ModContext` carries stable runtime state: mod identity, paths, settings, DLC data, and logger.
- `ModuleRegistry` owns gameplay modules and isolates module failures.
- `MiningYield`, `DuplicantBalance`, `DiseaseEffects`, `PowerGeneration`, `SolarGeneration`, `IndustrialHeat`, `ElectricalOverloadThermalDamage`, and `WorldGeneration` are independent modules. If one patch target is missing, the module logs the failure and the rest of the mod continues loading.
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

Until the options UI is implemented, enable gameplay by editing `config\hardcore_systems.json` in the installed mod folder. For runtime gameplay testing, enable `Mining`, `Duplicants`, `Diseases`, `Power.GeneratorRebalanceEnabled`, `Power.SolarPanelGenerationHeatEnabled`, `Power.OverloadHeatEnabled`, `IndustrialHeat.Enabled`, and optionally `Diagnostics.Enabled`, then restart ONI.

For v0.7 asteroid-size testing, create a new world and choose one of the generated `Hardcore ... Half` or `Hardcore ... Quarter` clusters. These presets are separate worldgen assets; they do not shrink existing saves or replace vanilla clusters. `Quarter` presets are more experimental: they target about 25% map area and use compact PowerTree defaults, compact subworld variants, and compact layout rules so starter, lava/core, surface/regolith, and resource biomes do not reserve full-size vanilla regions. Quarter uses a denser layout node grid, pins actual space to the top surface tag, keeps surface/regolith crust directly under it, and adds several optional geyser/vent placement attempts while avoiding mandatory warp, Gravitas, story, and large POI template guarantees. DLC Quarter worlds also add a shared compact DLC biome pool so vanilla-style Spaced Out starts can use swamp, rust, radioactive, wasteland, frozen, ocean, marsh, oil, and barren DLC subworlds when space exists.

Current `Power` settings:

```json
{
  "GeneratorRebalanceEnabled": true,
  "SolarPanelGenerationHeatEnabled": true,
  "OverloadHeatEnabled": true
}
```

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

Gameplay patches are registered through `PatchGuard`. Mining yield patches `WorldDamage.OnDigComplete`; calorie balance uses a runtime `Calories.deltaAttribute` modifier; generator rebalance patches `Generator.WattageRating`, `StructureTemperaturePayload.OperatingKilowatts`, `StructureTemperaturePayload.ExhaustKilowatts`, `StructureTemperaturePayload.TotalEnergyProducedKW`, `EnergyGenerator.EnergySim200ms(float)`, `EnergyGenerator.Emit(...)`, `ManualGenerator.EnergySim200ms(float)`, and `Building.EffectDescriptors` for the v0.4 generator profile table. Profiled generator native operating/exhaust heat is set to zero and their body heat is injected directly through ONI's building thermal API each simulation tick while active. For profiled `EnergyGenerator` buildings, consumed hot fuel also adds surplus heat to the generator body; direct and stored gas outputs use at least the captured hot fuel temperature; liquid outputs are capped below their phase transition temperature; heat assigned to outputs is subtracted from body surplus. Building heat scaling still patches `StructureTemperaturePayload.OperatingKilowatts`, `StructureTemperaturePayload.ExhaustKilowatts`, and `Building.EffectDescriptors`, but skips generators owned by `PowerGeneration`. Solar heat patches `SolarPanel.EnergySim200ms(float)` and `StructureTemperaturePayload.TotalEnergyProducedKW`; it adds heat from `SolarPanel.CurrentWattage` through ONI's building thermal simulation, shows current generation heat in hover, and shows maximum generation heat in static descriptors. Electrical overload heating patches `BuildingHP.DoDamage`; it only handles overload damage metadata, checks `Wire`/`WireUtilityNetworkLink`/wire bridge interfaces, calculates energy from material melting point, building mass converted from kg to g, and SHC, then injects energy through `SimMessages.ModifyBuildingEnergy` when the building thermal sim handle is available. The mod does not patch saves directly.

## Testing

Run the included smoke tests through MSBuild:

```powershell
.\tools\build.ps1 -Configuration Release
.\src\HardcoreSystems.Tests\bin\Release\HardcoreSystems.Tests.exe
```

## Deployment

The local install script copies the compiled DLL, manifest files, localization files, and generated worldgen asset directories into:

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
- If generator rebalance is not visible, confirm `Power.GeneratorRebalanceEnabled=true` and test with Coal, Hydrogen, Natural Gas, Petroleum, Wood, Peat, or Manual generators. Steam Turbine, Solar Panel, tidal/reef generators, and dischargers are intentionally excluded from the fuel-generator profile table. Profiled generators should not inherit `IndustrialHeat.HeatMultiplier`.
- If building heat scaling is not visible on non-generators, test with a Research Station, Liquid Pump, or Gas Pump, confirm the tooltip heat value changed after restart, and increase `IndustrialHeat.HeatMultiplier` conservatively.
- If Solar Panel heat is not visible, confirm `Power.SolarPanelGenerationHeatEnabled=true`; test at different light levels and confirm generated heat follows actual wattage. Static effect descriptors show the maximum 5 kDTU/s value; hover heat should follow current wattage.
- If wire emergency heat is not visible, confirm `Power.OverloadHeatEnabled=true` and test with actual overload damage on a regular wire or a wire bridge.
- Wire resistance, length-based losses, voltage-drop, consumer brownouts, legacy generator efficiency, transformer efficiency, superconductive wires, and world-temperature presets are not planned for this project.
- Hot fuel now adds surplus heat to profiled generator bodies and products. Direct and stored gas outputs are corrected from captured hot fuel; liquid outputs are capped below phase transition so water and polluted water are not turned into steam inside an otherwise working generator.
