# Scarce Resource Balance

Mining Yield can make finite resources effectively non-renewable at low multipliers. Version 0.2.0 does not add recipes automatically; this document records candidates for later balancing.

| Resource | Source | Exhaustion risk | Proposed recipe | Tech level | Cost | Byproducts |
| --- | --- | --- | --- | --- | --- | --- |
| Abyssalite | Map tiles and some special sources | High at 10% and extreme at 1% | Molecular Forge conversion from refined carbon, tungsten, and diamond | Late game | Very high power and rare inputs | Heat |
| Fossil | Map tiles and fossil sites | Medium | Rock Crusher or Kiln conversion from sedimentary rock and lime | Mid game | Moderate mass loss | Sand |
| Wolframite | Cold biomes and space POIs | Medium | No recipe in 0.2.0; prefer POI/worldgen review first | Late game | TBD | TBD |
| Graphite | DLC-specific sources | Medium | No recipe in 0.2.0; avoid base-game dependency | Late game | TBD | TBD |
| Neutronium | Map boundary and geyser bases | Must remain unavailable | No recipe | None | None | None |

## Decision

Only Abyssalite is a strong candidate for a future expensive recipe. Neutronium must not become obtainable through this mod.

## Manual Checks

- Mine ordinary ore at each preset and confirm dropped mass is reduced once.
- Mine ice and confirm temperature/state behavior remains vanilla except for mass.
- Mine abyssalite and confirm it is affected only when the game itself emits mineable mass.
- Confirm neutronium remains unavailable.
