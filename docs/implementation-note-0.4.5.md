# Implementation Note 0.4.5

## Scope

0.4.5 addresses two in-game validation findings:

- profiled generator heat appeared in descriptions but did not produce the expected body heating;
- obsolete power configuration fields remained visible even though the current v0.4 concept no longer used them.
- hot fuel lost its thermal impact on the generator body.

## Generator Heat

`StructureTemperatureComponents` registers building heat exchange when the building thermal component is initialized. In practice, changing only `StructureTemperaturePayload.OperatingKilowatts` and descriptors was not enough to make existing generators emit the v0.4 heat values reliably.

0.4.5 changes profiled generators to:

- return `0` from `OperatingKilowatts` for the native heat-exchange path;
- inject profile body heat directly during `EnergyGenerator.EnergySim200ms(float)` and `ManualGenerator.EnergySim200ms(float)`;
- keep descriptors aligned with the v0.4 profile table.

Formula:

```text
EnergyDtu = BodyHeatKilowatts * 1000 * deltaTimeSeconds
```

## Hot Fuel

For profiled `EnergyGenerator` buildings, 0.4.5 captures fuel mass and temperature before vanilla consumption and compares the remaining mass after vanilla consumption. The surplus thermal energy of consumed hot fuel is added to the body:

```text
FuelSurplusDtu =
    ConsumedMassKg * 1000 * FuelSHC * max(0, FuelTemperatureK - BodyTemperatureK)
```

This intentionally does not change product mass, product type, or product output temperature yet.

## Configuration Cleanup

The active `Power` config now contains only:

```json
{
  "GeneratorRebalanceEnabled": true,
  "SolarPanelGenerationHeatEnabled": true,
  "OverloadHeatEnabled": true
}
```

Removed from active config and source:

- legacy generator efficiency;
- electrical losses;
- transformer efficiency;
- wire resistance;
- fuel thermal accounting switch before implementation;
- electrical diagnostics switch;
- overload heat mode switch.

## Remaining Work

Product temperature correction is still not active. Hot fuel now heats the generator body, but emitted water and CO2 can still use vanilla output temperatures until generator output hooks are implemented and validated.
