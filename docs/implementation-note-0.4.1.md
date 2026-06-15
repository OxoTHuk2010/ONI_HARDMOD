# Implementation Note 0.4.1

## Scope

Version 0.4.1 replaces the first experimental electrical implementation with a path-based model:

- wire resistance depends on route length;
- inferred material and wire capacity affect resistance;
- wire temperature affects resistance;
- consumer brownouts approximate reduced delivered power;
- wire damage produces a short-circuit heat impulse;
- transformer efficiency patches are disabled.

## Game Hooks

- `Wire.OnSpawn` and `Wire.OnCleanUp` maintain a local active-wire registry.
- `EnergyConsumer.EnergySim200ms(float dt)` runs after ONI's vanilla power update and can clear the powered flag for brownouts.
- `CircuitManager.Sim200msFirst(float dt)` applies bounded overload heat to sampled wires.
- `BuildingHP.DoDamage(int damage)` adds a heat impulse when the damaged building has a `Wire` component.

## Runtime Model

For each powered consumer, the module finds the shortest wire path from the consumer power cell to a source cell on the same circuit. Source cells are generators and charged non-transformer batteries.

Per-wire resistance is calculated from:

```text
resistance = baseOhmsPerCell * configMultiplier * materialFactor * temperatureFactor * capacityFactor
```

Path loss uses an Ohm-style approximation:

```text
current = requiredWatts / nominalVoltage
lossWatts = current^2 * pathResistance * nominalVoltage
availability = (requiredWatts - lossWatts) / requiredWatts
```

ONI generally does not expose a generic partial-power speed scalar for all buildings, so reduced power is approximated by deterministic duty cycling:

- availability below 50% keeps the consumer unpowered;
- availability from 50% to 99% powers the consumer for a proportional number of simulation slots;
- availability near 100% leaves vanilla behavior intact.

## Material Model

The game does not expose a single electrical resistivity property for wire materials, so v0.4.1 infers a factor from `PrimaryElement.ElementID`:

- Aluminum: `0.45`
- Gold: `0.60`
- Copper: `0.75`
- Steel: `0.85`
- Lead: `1.40`
- Unknown/default: `1.00`

Wire wattage rating also reduces resistance for higher-capacity wires.

## Limits

This is still an ONI-compatible approximation, not a full circuit solver. The module does not rewrite generator dispatch or transformer behavior. If a consumer's power cell is not represented in the wire registry, the module skips that consumer rather than forcing behavior.

## Verification

- `tools\build.ps1 -Configuration Release -OniGameDir "D:\SteamLibrary\steamapps\common\OxygenNotIncluded"` completes through the existing direct compiler fallback.
- `src\HardcoreSystems.Tests\bin\Release\HardcoreSystems.Tests.exe` returns `All tests passed.`
