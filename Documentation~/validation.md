# Validation

Phase 2L validation should cover:

- Unity compile in a fresh validation project.
- EditMode tests for descriptor creation, draft determinism, spawn, weapon damage/death, XP collection, upgrade selection, and magnet recall.
- PlayMode tests for first playable boot, run continuation after upgrade, player death, and restart.
- Manual sample scene open/play check from `Samples~/BasicSurvivorsGame/Scenes/BasicSurvivorsGame.unity`.

The reference Vampire clone has local working-tree edits in UI files during this phase and is treated as read-only input.

Phase 2M adds validation for:

- local weapon archetype descriptor coverage
- orbit upgrade application
- orbit weapon damage/death
- melee weapon damage/death
- burst weapon damage/death
- run upgrade affecting a new weapon archetype

Phase 2N adds validation for:

- runtime catalog content validation
- sample JSON load and validation for weapons/projectiles/upgrades
- clear validation failures for duplicate weapon IDs, duplicate upgrade IDs, invalid archetypes, missing projectile references, and invalid upgrade targets
- hitscan/beam damage and enemy death
- projectile pierce behavior
- projectile chain retarget behavior
- projectile fork/split spawn behavior
- projectile return/boomerang behavior
- run upgrade affecting a new projectile modifier

No package extraction, package publishing, Survivors template registration, Idle template mutation, or Movement-FPS migration is part of Phase 2N validation.
