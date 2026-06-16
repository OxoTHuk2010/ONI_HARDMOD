# Implementation Note 0.4.7

## Scope

0.4.7 fixes hover heat values and implements safe product-temperature correction.

## Hover Heat

ONI hover heat uses `StructureTemperaturePayload.TotalEnergyProducedKW`, not only `Building.EffectDescriptors`.

0.4.7 patches this getter:

- Solar Panel returns current generation heat:

```text
clamp(CurrentWattage / 380, 0, 1) * 5 kDTU/s
```

- Profiled generators return their v0.4 body heat profile while active.

This keeps static properties and hover values aligned without re-enabling vanilla native heat exchange for profiled generators.

## Product Temperature

For profiled `EnergyGenerator` buildings, the same captured fuel state used for body surplus heat is now used by `EnergyGenerator.Emit(...)`.

When a direct output element is gas:

```text
OutputMinTemperature = max(VanillaMinTemperature, CapturedFuelTemperature)
```

When a direct output element is liquid:

```text
OutputMinTemperature = min(CapturedFuelTemperature, Element.highTemp - 1 K)
```

For stored outputs, 0.4.7 also scans the generator `Storage` after `Emit(...)` and raises matching stored product `PrimaryElement.Temperature` to the same safe target. This covers product paths that do not use `OutputItem.minTemperature` directly.

The heat assigned to products is tracked and subtracted from the hot-fuel body surplus. This keeps product mass/type stable, avoids turning water or polluted water into steam inside a healthy generator, and avoids counting the same surplus heat both in the output and in the generator body.
