# Implementation Note 0.4.2

## Scope

Version 0.4.2 narrows the electrical-network scope to one mechanic:

- when a wire or wire bridge takes building damage, heat it near the melting point of its construction material;
- do not cross the material phase transition;
- keep vanilla power routing, generation, transformer behavior, and consumer speed behavior.

The following ideas are intentionally inactive:

- wire resistance;
- length-based losses;
- voltage-drop;
- power routing;
- current distribution between branches;
- consumer brownouts or production slowdown;
- transformer efficiency;
- global generator output reduction.

## Game Hooks

- `BuildingHP.DoDamage(int damage)` is patched with a postfix.
- The postfix checks whether the damaged object has `Wire` or `WireUtilityNetworkLink`.
- If the object has `PrimaryElement`, the module reads `PrimaryElement.Element.highTemp`.

## Temperature Target

The target is 90% of the material melting point measured in Celsius, converted back to Kelvin for ONI:

```text
targetK = 273.15 + (highTempK - 273.15) * 0.90
targetK <= highTempK - 5K
```

This matches the intended behavior: a damaged iron-ore wire should be heated close to its melting point, but should remain safely below the phase transition.

If the wire is already hotter than the target, the module does not cool it.

## Bridge Support

Wire bridges use `WireUtilityNetworkLink`, not `Wire`. Version 0.4.2 checks both components so bridge damage receives the same emergency heat behavior as normal wire damage.

## Verification

- `tools\build.ps1 -Configuration Release -OniGameDir "D:\SteamLibrary\steamapps\common\OxygenNotIncluded"` completes through the existing direct compiler fallback.
- `src\HardcoreSystems.Tests\bin\Release\HardcoreSystems.Tests.exe` returns `All tests passed.`
