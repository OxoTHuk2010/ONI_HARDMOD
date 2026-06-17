# Changelog

## 0.7.7

- Quarter Spaced Out vanilla-style worlds now add a shared compact DLC biome pool instead of relying only on the source world's vanilla-style subworld list.
- The DLC pool is limited to DLC-generated worlds and is checked against existing `expansion1` subworld files before insertion, so base-game Quarter presets do not gain hard DLC references.
- Added compact variants for selected swamp, rust, radioactive, wasteland, frozen, ocean, forest, marsh, jungle, oil, and barren DLC subworlds to improve biome variety on 20-25% maps.

## 0.7.6

- Re-tuned Quarter world layout after in-game testing showed only three oversized biomes on generated maps.
- Quarter now uses much lower PowerTree density values (`OverworldDensityMin: 6`, `OverworldDensityMax: 10`) to create many smaller biome pockets instead of a few huge regions.
- Quarter now separates actual `Space` from `Surface`/`Regolith`: space is pinned with `AtTag AtSurface`, while crust/regolith is limited to one band below it, reducing sideways surface placement and neutronium-closed tops.
- Quarter now adds up to six optional geyser/vent attempts through relaxed `TryOne` rules, preserving generation stability while giving compact maps more resource variety when placement space exists.

## 0.7.5

- Fixed Quarter world generation failure introduced in 0.7.4: `OverworldMinNodes: 24` could abort generation with `World layout with fewer than 24 points`.
- Quarter keeps the 0.7.4 small-biome footprint tuning, but lowers the hard minimum to `OverworldMinNodes: 1`, matching Klei tiny-world presets.

## 0.7.4

- Tightened Quarter biome footprint after in-game testing showed average biome regions around 40x60 cells.
- Quarter worlds now force compact PowerTree defaults: `OverworldDensityMin: 80`, `OverworldDensityMax: 90`, `OverworldAvoidRadius: 2`, `OverworldMinNodes: 24`, and `OverworldMaxNodes: 800`.
- Quarter compact subworlds now use `avoidRadius: 2` and a forced `pdWeight: 0.2` on every generated compact subworld, including subworlds that did not define `pdWeight` in vanilla YAML.
- Quarter magma/core and space/regolith placement now use `DistanceFromTag` with `minDistance: 0` and `maxDistance: 0` instead of broad `AtTag` replacement, reducing oversized lava and surface bands.

## 0.7.3

- Reworked `Quarter` world layout rules for playability instead of only generation success.
- Quarter worlds now replace inherited full-size `unknownCellsAllowedSubworlds` with compact rules: one small starter ring, mixed mid-biome pockets, magma only at `AtDepths`, and space/regolith only at `AtSurface`.
- Quarter generated subworlds now use `avoidRadius: 3`; subworlds that define `pdWeight` are reduced to `0.5`, and inherited `overridePower` is removed from Quarter world `subworldFiles`.
- Quarter now adds one optional water-focused geyser/vent `TryOne` rule instead of inherited guaranteed POI/template blocks, keeping water content possible without making template placement a hard failure.

## 0.7.2

- Reworked generated `Quarter` asteroid presets after in-game tests showed repeatable `TemplateSpawning: Guaranteed placement failure` on quarter-size worlds.
- Quarter worlds now remove inherited `worldTemplateRules` in addition to world traits, subworld mixing, and min/max subworld count guarantees.
- Quarter worlds now use generated compact subworld variants with lower placement pressure for start and biome regions.
- Quarter is still experimental: it prioritizes getting a compact asteroid generated over preserving guaranteed geyser, warp, Gravitas, and story/POI template placement.

## 0.7.1

- Fixed generated reduced-world YAML by removing inherited `minCount`/`maxCount` guarantees from generated `subworldFiles` and `subworldMixingRules`.
- Disabled inherited world traits and removed generated `subworldMixingRules` from reduced worlds to reduce small-map placement pressure.
- The previous 0.7.0 assets could fail every generation attempt on small maps with `Could not guarantee minCount of Subworld ...` because vanilla subworld guarantees were too strict for `Half` and especially `Quarter` asteroid sizes.
- Kept generated clusters separate from vanilla presets and kept Spaced Out classic outer worlds vanilla.

## 0.7.0

- Added experimental data-only asteroid-size presets for new worlds: `Half` and `Quarter`.
- Generated separate Hardcore Systems worldgen cluster/world YAML files instead of modifying vanilla presets in place.
- Covered base-game vanilla asteroid starts and Spaced Out classic/vanilla-style starts; SpacedOutStyle mini-clusters remain excluded from automatic shrinking until seed testing.
- Removed obsolete world temperature fields from the active config model and validator.
- Added installer support for mod `worldgen` and `dlc` asset directories.

## 0.4.7

- Patched `StructureTemperaturePayload.TotalEnergyProducedKW` for Solar Panel and profiled generators so hover heat matches v0.4 runtime behavior.
- Solar Panel hover heat now uses current wattage: `CurrentWattage / 380 * 5 kDTU/s`.
- Profiled generator hover heat now reports the v0.4 body heat profile instead of 0 kDTU/s.
- Added product temperature correction for profiled fuel generators: direct and stored gas outputs use at least the captured hot fuel temperature; liquid outputs are capped below their phase transition temperature; output-assigned heat is subtracted from body surplus to avoid double counting.

## 0.4.6

- Fixed `IndustrialHeat` leaking into Solar Panel and profiled generator descriptors.
- Suppressed profiled generator `ExhaustKilowatts` in runtime and descriptor paths so generator heat is no longer `profile body heat + vanilla exhaust heat * IndustrialHeat`.
- Fixed hot petroleum/ethanol fuel matching by accepting stored items that have the generator input tag, such as `CombustibleLiquid`, instead of requiring exact element tag equality.
- Expected static effect heat after this change: Solar Panel max 5 kDTU/s, Coal 40 kDTU/s, Hydrogen 32 kDTU/s, Natural Gas 20 kDTU/s, Petroleum 50 kDTU/s.

## 0.4.5

- Removed obsolete active configuration fields for legacy generator efficiency, electrical losses, transformer efficiency, wire resistance, fuel thermal accounting, electrical diagnostics, and overload heat mode.
- Removed legacy `GeneratorEfficiency` and `ElectricalNetworks` modules from the active source tree.
- Changed profiled generator body heat to an explicit runtime thermal injection during generator simulation ticks, avoiding reliance on ONI's building heat-exchange registration timing.
- Suppressed native operating heat for profiled generators so v0.4 generator heat is not mixed with vanilla heat or generic industrial heat scaling.
- Added minimal fuel thermal accounting for profiled `EnergyGenerator` buildings: heat surplus from consumed hot fuel is added to the generator body without changing product mass or type.
- Added Solar Panel maximum generation heat to building effect descriptors while keeping actual heat runtime-proportional to generated wattage.

## 0.4.4

- Added active `PowerGeneration` profiles for the v0.4 generator matrix: Manual, Wood, Coal, Peat, Hydrogen, Natural Gas, and Petroleum generators now report the configured wattage and body heat values.
- Excluded profiled generators from the generic `IndustrialHeat` multiplier so generator heat no longer becomes `vanilla heat * IndustrialHeat.HeatMultiplier`.
- Fixed emergency overload heat magnitude by converting `PrimaryElement.Mass` from kilograms to grams before applying the v0.4 `mass * SHC * deltaT` formula.
- Kept Steam Turbine, Solar Panel, tidal/reef generators, and electrobank dischargers outside the fuel-generator profile table.
- Added tests for generator profiles and runtime overload mass-unit conversion.

## 0.4.3

- Added the v0.4 research-layer split for `SolarGeneration` and `ElectricalOverloadThermalDamage`.
- Added Solar Panel generation heat: `SolarPanel.EnergySim200ms(float)` is patched and `SolarPanel.CurrentWattage` drives 0-5 kDTU/s heat from 0-380 W.
- Reworked overload wire heating to use `BuildingHP` overload damage metadata, wire/bridge conductor detection, material `Element.highTemp`, mass, and SHC.
- Replaced direct wire temperature assignment with energy injection through ONI building thermal simulation when a structure-temperature sim handle is available.
- Corrected overload target formula to `0.90 * meltingTemperatureK`, matching the v0.4 TODO.
- Added event deduplication per damaged object and frame.
- Added pure `SolarHeatCalculator` and `OverloadHeatCalculator` tests for the required v0.4 cases.
- Added `docs/v0.4-generator-runtime-audit.md` and `docs/v0.4-electrical-overload-audit.md`.
- Generator thermal rebalance remains in research/planning state; generator wattage and fuel/product behavior are unchanged in 0.4.3.

## 0.4.2

- Re-scoped Electrical Networks to only handle emergency thermal behavior on damaged wires and wire bridges.
- Removed active wire resistance, length-based losses, voltage-drop, brownout, and transformer-efficiency behavior.
- Disabled the active generator output reduction module for the v0.4 concept; generator wattage now stays vanilla unless a later generator thermal rebalance explicitly changes it.
- Damaged `Wire` and `WireUtilityNetworkLink` buildings now heat to 90% of their construction material melting point measured in Celsius, capped below the phase transition temperature.
- Presets no longer enable generator efficiency, electrical losses, or transformer efficiency; old config fields remain for compatibility.

## 0.4.1

- Reworked Electrical Networks around path-based losses instead of whole-circuit loss sampling.
- Electrical loss now depends on path length, configured resistance multiplier, inferred wire material, wire wattage rating, and wire temperature.
- Added brownout behavior for powered consumers: delivered power below 50% keeps the building unpowered; 50-99% uses a deterministic duty cycle to approximate reduced production speed.
- Added strong short-circuit heat impulse when a wire building takes damage, with smaller heat applied to adjacent wires.
- Disabled active transformer-efficiency patches; transformer settings remain in config for compatibility but are not applied.

## 0.4.0

- Added Electrical Networks module for transformer efficiency, wire resistance losses, and overload heat.
- Transformer efficiency now drains additional joules from the transformer input battery while preserving the output circuit delivery limit.
- Electrical losses can drain batteries on the affected circuit and dissipate a bounded amount of heat into sampled wires.
- Overloaded circuits add extra bounded heat to wires while the circuit load is above its safe wattage.
- Added transformer efficiency descriptor text and formula tests for electrical loss and transformer calculations.

## 0.3.0

- Added Generator Efficiency module using `Generator.WattageRating` to reduce generated power without changing fuel consumption, with matching building effect tooltip output.
- Added Industrial Heat module through ONI's structure-temperature operating/exhaust heat for any building with positive heat output, with matching building effect tooltip output.
- Added power-based fallback heat for pump buildings that have zero vanilla heat, such as gas pumps.
- Added optional runtime diagnostics metrics for module calls, timing, processed/skipped entities, and errors.

## 0.2.3

- Removed calorie tuning getter patches; calorie burn is now applied only through the Hardcore Systems runtime modifier.
- Prevented calorie modifier stacking when current burn is already at or above the configured target.
- Moved Mining Yield from generic `SimMessages` hooks to `WorldDamage.OnDigComplete`, before ONI applies its vanilla 50% mined-mass drop.

## 0.2.2

- Applied duplicant calorie burn as a runtime `Calories.deltaAttribute` modifier so existing saves reflect custom calorie multipliers.
- Switched Mining Yield Harmony patch arguments to positional injection for stable runtime argument binding.

## 0.2.1

- Fixed Mining Yield application for ONI's delayed dig mass emission path.
- Added one-shot dig cell tracking so the multiplier applies once to the emitted mass from a mined cell.

## 0.2.0

- Added Mining Yield, Duplicant Balance, and Disease Effects modules.
- Added fail-safe Harmony patch registration for mining mass, calorie burn, and disease database initialization.
- Added formula tests for mining yield, duplicant XP/calorie multipliers, and disease penalties.

## 0.1.0

- Added foundation mod project for Oxygen Not Included.
- Added loader, DLC detection, structured logging, module registry, safe patch wrapper, configuration DTOs, presets, validation, localization placeholders, build script, install script, and game API research notes.
- No gameplay patches are included in this stage.
