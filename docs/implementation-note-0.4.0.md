# Implementation Note 0.4.0

## Scope

Version 0.4.0 implements the first safe vertical slice of electrical network difficulty:

- transformer efficiency;
- wire resistance losses;
- bounded wire heat from normal losses;
- extra bounded wire heat while a circuit is overloaded;
- transformer efficiency descriptor text.

## Game Hooks

- `Wire.OnSpawn` and `Wire.OnCleanUp` maintain a local active-wire registry.
- `CircuitManager.Sim200msFirst(float dt)` processes up to 96 registered wires per simulation tick.
- `PowerTransformer.ApplyDeltaJoules(float,bool)` drains additional joules from the private input `Battery` before ONI applies vanilla output delivery.
- `Battery.GetDescriptors(GameObject)` appends an informational transformer efficiency descriptor.

## Runtime Model

Transformer efficiency keeps ONI's output delivery behavior intact and increases input-side cost:

```text
input = output / efficiency
extraInput = input - output
```

Electrical losses use circuit load and the configured resistance multiplier:

```text
loadRatio = wattsUsed / maxSafeWatts
lossWatts = wattsUsed * 0.02 * resistanceMultiplier * loadRatio^2
```

If a circuit has batteries, the loss drains those batteries once per circuit per processed tick. Loss and overload heat are also converted into small temperature deltas on sampled wires. Temperature deltas are capped per tick so a bad topology or unexpectedly high wattage cannot spike wire temperature in one simulation step.

## Limits

Direct generator-to-consumer circuits without batteries have no stored battery reserve to drain, so the visible loss is heat-only. The implementation intentionally avoids rewriting ONI's circuit topology or generator dispatch logic in this version.

## Verification

- `tools\build.ps1 -Configuration Release -OniGameDir "D:\SteamLibrary\steamapps\common\OxygenNotIncluded"` completes through the existing direct compiler fallback.
- `src\HardcoreSystems.Tests\bin\Release\HardcoreSystems.Tests.exe` returns `All tests passed.`
