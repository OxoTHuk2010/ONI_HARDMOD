# Changelog

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
