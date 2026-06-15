# Changelog

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
