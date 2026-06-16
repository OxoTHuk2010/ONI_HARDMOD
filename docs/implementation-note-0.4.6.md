# Implementation Note 0.4.6

## Scope

0.4.6 fixes UI and runtime heat mixing between v0.4 power modules and the generic `IndustrialHeat` module.

## Fixes

- Solar Panel descriptors are excluded from `IndustrialHeat`, so the static maximum generation heat is 5 kDTU/s instead of 7.5 kDTU/s when `IndustrialHeat.HeatMultiplier=1.5`.
- Profiled generators suppress both native operating heat and native exhaust heat in runtime structure-temperature paths.
- Profiled generator descriptors suppress vanilla exhaust heat, so Coal/Hydrogen/Petroleum show only their v0.4 body heat profile.
- Fuel thermal sampling now accepts stored items that carry the generator input tag. This matters for Petroleum Generator because ONI uses the generic `CombustibleLiquid` input tag rather than exact `Petroleum`.

## Expected Static Heat Values

| Building | Static effect heat |
| --- | ---: |
| Solar Panel | 5 kDTU/s maximum |
| Coal Generator | 40 kDTU/s |
| Hydrogen Generator | 32 kDTU/s |
| Natural Gas Generator | 20 kDTU/s |
| Petroleum Generator | 50 kDTU/s |

Solar Panel actual runtime heat remains proportional to current wattage:

```text
ActualHeat = clamp(CurrentWattage / 380, 0, 1) * 5 kDTU/s
```
