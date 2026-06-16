# Implementation Note 0.4.3

## Scope

Version 0.4.3 aligns the implemented electrical and solar behavior with `FIX_TODOv0.4.txt` while keeping generator thermal rebalance in research state.

Implemented:

- `SolarGeneration` module;
- `ElectricalOverloadThermalDamage` module;
- pure `SolarHeatCalculator`;
- pure `OverloadHeatCalculator`;
- DTO/profile scaffolding for v0.4 audit work;
- docs audits for generator runtime and electrical overload.

## Electrical Overload

The overload target formula is now:

```text
TargetTemperatureK = 0.90 * MeltingTemperatureK
RequiredEnergyDtu = MassGrams * SHC * max(0, TargetTemperatureK - CurrentTemperatureK)
```

Runtime no longer assigns `PrimaryElement.Temperature` directly. It injects energy through ONI's building thermal simulation when the structure-temperature sim handle is available.

## Solar Panel

The solar formula is:

```text
SolarHeatDtuPerSecond = clamp(SolarPanel.CurrentWattage / 380, 0, 1) * 5000
EnergyDtu = SolarHeatDtuPerSecond * dt
```

The module does not scan light sources and does not distinguish sunlight from Shine Bugs or lamps.

## Known Limits

- Generator thermal rebalance is not active.
- Wire tooltips and Solar Panel tooltips from the TODO are not implemented yet.
- In-game overload tests are still required for joint plates and any DLC conductors.
- If ONI has not registered a building thermal sim handle for a conductor, heat is skipped with a rate-limited warning.
