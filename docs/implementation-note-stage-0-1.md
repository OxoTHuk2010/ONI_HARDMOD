# Implementation Note: Stages 0 and 1

## What is implemented

- Minimal ONI KMod loader through `KMod.UserMod2`.
- DLC directory detection through Unity `Application.dataPath`.
- Structured Unity logger with stable event names and fields.
- Rate-limited warning support.
- `ModuleRegistry` with per-module failure isolation.
- Configuration DTOs for global, world, mining, heat, power, fluid pressure, disease, duplicant, and diagnostics settings.
- Presets `Off`, `VanillaPlus`, `Hard`, `Extreme`, `Insane`, and `Custom`.
- Settings validation and schema version storage.
- English and Russian localization placeholders.
- Build and local install scripts.
- Fallback build reference stubs for machines where MSBuild exists without Roslyn C# targets.

## Changed files

- `ONI.HardcoreSystems.sln`
- `Directory.Build.props`
- `src/HardcoreSystems.Mod/**`
- `src/HardcoreSystems.Tests/**`
- `docs/game-api-research.md`
- `tools/build.ps1`
- `tools/install-local.ps1`
- `tools/stubs/**`
- `README.md`
- `CHANGELOG.md`
- `mod.yaml`
- `mod_info.yaml`

## Risks

- The local Visual Studio installation must include the C# compiler targets. MSBuild is present, but the C# workload may still be incomplete.
- Fallback stubs cover only the stage 0/1 API surface and must not be shipped in the local mod folder.
- ONI mod option UI integration is not implemented in this stage.
- Save-specific configuration has DTO support only; no live save hook is installed yet.
- DLC detection is directory-based until an official runtime DLC API is selected.

## Safety boundary

No gameplay patches are registered. `PatchGuard` is present for later modules and must be used for every Harmony patch.
