# Implementation Note: 0.2.0 Duplicant and Mining

## What is implemented

- `MiningYield` patches `SimMessages.Dig` and `SimMessages.EmitMass`.
- `DuplicantBalance` applies skill XP tuning and patches base calorie burn getters.
- `DiseaseEffects` adds sickness components after `Db.Initialize`.
- Formula-level tests cover mining mass, XP/calorie multipliers, and disease penalty tiers.

## Files changed

- `src/HardcoreSystems.Mod/Modules/MiningYield/**`
- `src/HardcoreSystems.Mod/Modules/DuplicantBalance/**`
- `src/HardcoreSystems.Mod/Modules/DiseaseEffects/**`
- `src/HardcoreSystems.Mod/Bootstrap/ModBootstrap.cs`
- `src/HardcoreSystems.Tests/Program.cs`
- `docs/scarce-resource-balance.md`

## Risks

- Mining mass assumes dig-generated drops flow through `SimMessages.EmitMass` during `SimMessages.Dig`.
- Skill XP uses `TUNING.SKILLS` gain constants because direct `MinionResume` methods were not safely exposed by reflection-only metadata.
- Disease modifiers use known attribute IDs: `WorkSpeed`, `QualityOfLife`, and `StressDelta`; these require in-game confirmation.
- Existing configs remain `Off` by default, so users must enable presets or module flags in `config\hardcore_systems.json` until a UI exists.

## Manual Test

1. Enable a preset such as `Hard` in the config.
2. Start a new colony or load a test save.
3. Dig sandstone/copper/ice and compare dropped mass with vanilla.
4. Observe calorie burn in the vitals UI and confirm the value scales by preset.
5. Let duplicants accrue skill XP over multiple cycles and compare against vanilla.
6. Infect one duplicant with Food Poisoning and one with Slimelung in a sandbox save; confirm modifier tooltips and recovery behavior.
