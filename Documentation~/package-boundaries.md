# Package Boundaries

## Existing Package Usage

- `Combat`: health state and damage resolution.
- `World Spawning`: pooled enemy, pickup, and projectile object lifecycle.
- `Weapon Systems`: stable weapon descriptor for the first auto-fire projectile weapon.
- `Projectiles`: stable projectile descriptor for the first projectile.
- `Run Upgrades`: deterministic draft generation, rank tracking, and max-rank filtering.
- `Encounters`: first-slice encounter/wave descriptors for authored spawn-flow shape.

## Local Survivors Kit Code

The following intentionally stay local in Phase 2L:

- top-down player movement
- camera follow
- radial spawn position rules
- nearest-enemy auto-targeting
- projectile steering and collision checks
- XP pickup attraction
- magnet recall
- level-up overlay
- concrete upgrade effect application

## Package Expansion Candidates Later

Do not extract these yet. Revisit only after parity gaps repeat across real templates or products:

- generalized horde escalation
- pooled projectile impact adapters
- richer Survivors weapon archetype descriptors
- XP/drop lifecycle abstractions
- content validation for authored Survivors libraries
- diagnostics for spawn pressure, pool stats, and draft state
