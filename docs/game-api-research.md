# Game API Research

Game path analyzed:

```text
D:\SteamLibrary\steamapps\common\OxygenNotIncluded
```

## Environment

- Managed assemblies: `OxygenNotIncluded_Data\Managed`
- Native simulation plugin: `OxygenNotIncluded_Data\Plugins\x86_64\SimDLL.dll`
- Harmony: `0Harmony.dll`, assembly version `2.4.2.0`
- Game assemblies: `Assembly-CSharp.dll` and `Assembly-CSharp-firstpass.dll`, assembly version `0.0.0.0`
- Unity modules: `UnityEngine.dll`, `UnityEngine.CoreModule.dll`, assembly version `0.0.0.0`
- Newtonsoft.Json: assembly version `7.0.0.0`
- VYaml: assembly version `1.0.0.0`
- Runtime profile observed from shipped assemblies: `mscorlib 4.0.0.0`, `netstandard 2.1.0.0`
- `0Harmony.dll` in this ONI build targets `.NETFramework,Version=v4.8`; the mod project targets .NET Framework 4.8.
- PLib: not found in the current game `Managed` directory; not used in stage 0/1.
- The Cairath ONI modding guide notes that gameplay modding is patch-based rather than a stable game API; this reinforces the fail-safe `PatchGuard` approach. Source: https://github.com/Cairath/Oxygen-Not-Included-Modding/wiki

## Reference Assemblies Used

Normal MSBuild path:

- `0Harmony.dll`
- `Assembly-CSharp.dll`
- `Assembly-CSharp-firstpass.dll`
- `Newtonsoft.Json.dll`
- `UnityEngine.dll`
- `UnityEngine.CoreModule.dll`
- .NET Framework `System`
- .NET Framework `System.Core`

Fallback compiler paths:

- direct Roslyn fallback uses real ONI reference assemblies;
- legacy fallback stubs named `0Harmony.dll`, `Assembly-CSharp.dll`, and `UnityEngine.CoreModule.dll` live under `tools\stubs`;
- stubs are compile-only and are not copied by `tools\install-local.ps1`.

## Confirmed API Points

| Area | Type | Method or member | Current stage use | Patch plan |
| --- | --- | --- | --- | --- |
| Mod loading | `KMod.UserMod2` | `OnLoad(HarmonyLib.Harmony)` | Entry point | None |
| Mod manager | `KMod.Manager` | `Load`, `Unload`, `GetDirectory`, `safe_mode_enabled` | Research only | None |
| Buildings | `GeneratedBuildings` | `LoadGeneratedBuildings(List<Type>)` | Research only | Prefix/postfix only after prototype |
| Recipes | `ComplexRecipeManager`, `ComplexRecipe+RecipeElement` | Manager and recipe element types found | Research only | Data registration preferred |
| Worldgen | `ProcGenGame.WorldGen` | Type found | Research only | YAML/data first |
| Worldgen settings | `ProcGen.WorldGenSettings` | Type found | Research only | YAML/data first |
| Cluster layout | `ProcGen.ClusterLayouts` | Type found | Research only | YAML/data first |
| Electrical wires | `Wire`, `Wire+WattageRating` | `OnSpawn`, `OnCleanUp`, `NetworkID`, `MaxWattageRating`, `GetMaxWattageAsFloat` | Version 0.4.1 | Register active wires, build cell topology, infer path/capacity/material resistance, and process bounded overload heat |
| Generators | `Generator`, `EnergyGenerator`, `ManualGenerator`, `Building` | `Generator.get_WattageRating`, `Building.EffectDescriptors`, `Generator.GenerateJoules`, `EnergyGenerator.EnergySim200ms`, `ManualGenerator.EnergySim200ms` | Version 0.3.0 | Postfix `WattageRating`; descriptor prefix mirrors the reduced `BuildingDef.GeneratorWattageRating` for tooltip text |
| Transformers | `PowerTransformer`, `Battery` | `PowerTransformer.ApplyDeltaJoules(float,bool)`, private `battery`, `Battery.ConsumeEnergy(float)`, `Battery.GetDescriptors(GameObject)` | Research only | Active transformer efficiency was disabled in 0.4.1 because ONI does not expose useful transformation behavior for the intended mechanic |
| Diseases | `Klei.AI.Disease`, `Klei.AI.Sickness`, `Klei.AI.SlimeSickness`, `Klei.AI.FoodSickness` | Type found | Research only | Later stage |
| Duplicants | `MinionResume`, `MinionIdentity`, `ModifierSet` | Type found | Research only | Later stage |
| Calories | `CreatureCalorieMonitor`, `CaloriesDisplayer`, `Database.*Calories*` | Type found | Research only | Later stage |
| Sim messages | `SimMessages` | `AddElementChunk`, `ModifyElementChunkEnergy`, `AddBuildingHeatExchange`, `ModifyBuildingEnergy`, and related methods | Research only | Avoid unless prototype proves safe |
| Mining yield | `WorldDamage` | `OnDigComplete(int,float,float,ushort,byte,int)` | Version 0.2.3 | Prefix mass before ONI applies vanilla 50% dig output |
| Calories | `MinionModifiers`, `Db.Get().Amounts.Calories.deltaAttribute` | `MinionModifiers.OnSpawn` | Version 0.2.3 | Add bounded runtime modifier; no tuning getter patch |
| Skill XP | `TUNING.SKILLS` | `FULL_EXPERIENCE`, `ALL_DAY_EXPERIENCE`, `MOST_DAY_EXPERIENCE`, `PART_DAY_EXPERIENCE`, `BARELY_EVER_EXPERIENCE` | Version 0.2.0 | One-time tuning multiplier |
| Disease effects | `Db`, `Database.Sicknesses`, `Klei.AI.Sickness` | `Db.Initialize`, non-public `Sickness.AddSicknessComponent` | Version 0.2.0 | Postfix plus guarded reflection |
| Building heat scaling | `StructureTemperaturePayload`, `Building` | `StructureTemperaturePayload.get_OperatingKilowatts`, `StructureTemperaturePayload.get_ExhaustKilowatts`, `Building.EffectDescriptors` | Version 0.3.0 | Postfix operating/exhaust KW for ONI's structure-temperature sim; descriptor prefix mirrors scaled self/exhaust heat for any positive heat output; pump zero-heat fallback uses power consumption |
| Electrical circuits | `CircuitManager`, `EnergyConsumer`, `BuildingHP` | `Sim200msFirst(float)`, `GetWattsUsedByCircuit(ushort)`, `GetMaxSafeWattageForCircuit(ushort)`, `GetBatteriesOnCircuit(ushort)`, `GetGeneratorsOnCircuit(ushort)`, `EnergyConsumer.EnergySim200ms(float)`, `BuildingHP.DoDamage(int)` | Version 0.4.1 | Path-based brownout for consumers, bounded overload heat, and short-circuit heat impulse on wire damage |
| Diagnostics | `System.Diagnostics.Stopwatch`, mod logger | n/a | Version 0.3.0 | Optional metrics recorded only when enabled |

## Sample Signatures Observed

```csharp
void KMod.UserMod2.OnLoad(HarmonyLib.Harmony harmony)
void GeneratedBuildings.LoadGeneratedBuildings(List<Type> types)
void SimMessages.AddElementChunk(int gameCell, SimHashes element, float mass, float temperature, float surface_area, float thickness, float ground_transfer_scale, int cb_handle)
void SimMessages.ModifyElementChunkEnergy(int sim_handle, float delta_kj)
void SimMessages.AddBuildingHeatExchange(Extents extents, float mass, float temperature, float thermal_conductivity, float operating_kw, ushort elem_idx, int callbackIdx)
void SimMessages.ModifyBuildingEnergy(int sim_handle, float delta_kj, float min_temperature, float max_temperature)
```

## Compatibility Risk By Mechanic

| Mechanic | Primary API direction | Alternative | Risk |
| --- | --- | --- | --- |
| Asteroid size | YAML worldgen templates | `ProcGenGame.WorldGen` patch | High |
| Biome temperature | YAML worldgen data | worldgen runtime patch | High |
| Mining yield | dig/mining result patch | resource spawn result patch | Medium |
| Radiative heat | registered emitter component and bounded processing | `SimMessages` energy calls | High |
| Electrical losses | electrical network model | wire/building energy patch | High |
| Wire overload heat | overload state hooks | building heat exchange | Medium |
| Transformer efficiency | `PowerTransformer` behavior | network delivery patch | Medium |
| New material | element registration data | direct element loader patch | Medium |
| New wires | building config/data registration | `GeneratedBuildings` patch | Medium |
| Generator efficiency | `Generator.WattageRating` plus descriptor mirror | `Generator.GenerateJoules` prefix | Medium |
| Building heat scaling | `StructureTemperaturePayload.OperatingKilowatts`/`ExhaustKilowatts` plus descriptor mirror | direct `SimMessages.ModifyBuildingEnergy` injection | Medium |
| Liquid pressure locks | barrier registry and limited SimMessages | raw cell simulation | High |
| Disease severity | `Klei.AI.Sickness`/disease DB | exposure patch | Medium |
| Experience speed | `MinionResume`/skill progression | modifier patch | Medium |
| Calories | `CreatureCalorieMonitor`/calorie attributes | chore/eat patch | Medium |

## Open Questions

- Exact public DLC API was not found as `DlcManager`; current stage uses directory detection.
- The current ONI build appears to ship Unity 6-era modules, but the exact Unity version must be confirmed at runtime through `Application.unityVersion`.
- Save-specific config hooks require a dedicated save/load API pass before implementation.
- Mod options UI should be researched separately before adding PLib or a custom screen.
- Reflection-only analysis can expose type names, but exact signatures for several gameplay types need deeper decompilation before gameplay modules.
- Base game vs Spaced Out behavior still needs an in-game smoke test because this environment cannot toggle DLC state without launching ONI.

## Stage 0/1 Decision

Use the smallest load-only foundation:

- no gameplay Harmony patches;
- no PLib dependency;
- directory-based DLC detection;
- JSON global config stored under the mod folder;
- presets and validation implemented outside game behavior;
- all future patches must use `PatchGuard.TryPatch`.

## Build Result

- `tools\build.ps1 -Configuration Release -OniGameDir "D:\SteamLibrary\steamapps\common\OxygenNotIncluded"` completed successfully through fallback compilation.
- Smoke tests completed successfully: `All tests passed.`
- Known warning: MSBuild may terminate Roslyn with `MSB5021`; `tools\build.ps1` falls back to direct Roslyn invocation with the same ONI references.
