# Implementation Note: 0.3.0 Generator Balance

## Scope

- `GeneratorEfficiency` reduces generator output through `Generator.WattageRating` and mirrors the reduced value in building effect descriptors.
- `IndustrialHeat` scales ONI's `StructureTemperaturePayload.OperatingKilowatts` and `ExhaustKilowatts` for any building with positive heat output, then mirrors the scaled total in building effect descriptors.
- `DiagnosticsRuntime` records optional module metrics when `Diagnostics.Enabled` is true.

## Decisions

- Generator fuel consumption is not changed in 0.3.0. ONI generators call `WattageRating` before `GenerateJoules`, so the module changes delivered power without touching fuel conversion formulas.
- Heat scaling intentionally follows ONI's existing positive heat-output fields instead of a hand-maintained building allowlist.
- Pump buildings with zero vanilla heat use a fallback of `EnergyConsumptionWhenActive / 120`, so a 240 W gas pump receives 2 kDTU/s before the configured multiplier and a 60 W mini gas pump receives 0.5 kDTU/s.
- Heat is applied through ONI's structure-temperature path instead of a separate `PrimaryElement` temperature write, so tooltip output and actual heat simulation use the same multiplier.

## Manual Test

1. Enable `Power.GeneratorEfficiencyEnabled=true` and set `Power.GeneratorEfficiency=0.5`.
2. Start ONI and confirm `GeneratorEfficiency` registers patches on `Generator.get_WattageRating` and `Building.EffectDescriptors`.
3. Run a coal or hydrogen generator and confirm displayed/output wattage and tooltip wattage are reduced.
4. Enable `IndustrialHeat.Enabled=true`, set `IndustrialHeat.HeatMultiplier=2.0`, and restart ONI.
5. Confirm a Coal Generator, Research Station, Liquid Pump, or Gas Pump tooltip shows increased heat and the building warms while active.
6. Enable `Diagnostics.Enabled=true` to confirm `diagnostics_module_metrics` appears after the configured interval.

## Risks

- Some generator-like DLC buildings may bypass `Generator.WattageRating`; if so, add a targeted patch after log-based verification.
- Buildings that generate heat through a custom path outside `StructureTemperaturePayload` may need a targeted patch after log-based verification.
