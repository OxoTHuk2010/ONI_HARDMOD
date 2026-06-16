# Implementation Note 0.4.4

## Scope

0.4.4 fixes two issues observed during in-game validation:

- overload wire heat was much weaker than the v0.4 formula because runtime passed `PrimaryElement.Mass` as grams even though ONI reports building mass in kilograms;
- generator rebalance was not active, so generator heat was still controlled by the generic `IndustrialHeat` multiplier.

## Changes

- Added `Modules/PowerGeneration`.
- Added v0.4 generator profiles for Manual, Wood, Coal, Peat, Hydrogen, Natural Gas, and Petroleum generators.
- Patched `Generator.WattageRating`, `StructureTemperaturePayload.OperatingKilowatts`, and `Building.EffectDescriptors` for profiled generators.
- Made `IndustrialHeat` skip profiled generators when `Power.GeneratorRebalanceEnabled=true`.
- Converted overload conductor mass from kg to g before calculating `mass * SHC * deltaT`.

## Not Changed

- Fuel consumption, product mass, and product type are still vanilla.
- Fuel thermal accounting remains disabled by default and is not implemented in runtime yet.
- Steam Turbine, Solar Panel, tidal/reef generators, compact discharger, and large discharger remain excluded from fuel-generator profiles.
- Wire resistance, voltage drop, and length-based losses remain inactive.
