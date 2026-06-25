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

Phase 2O adds validation for:

- payload runtime descriptor coverage
- payload sample JSON timing and radius validation
- clear validation failures for invalid payload count, travel speed, arming time, lifetime, trigger radius, explosion radius, hazard duration, hazard tick interval, and negative hazard damage ratio
- grenade payload detonation, area damage, and enemy death
- placed trap arming/proximity trigger, area damage, and enemy death
- run upgrade affecting payload behavior

No package extraction, package publishing, Survivors template registration, Idle template mutation, or Movement-FPS migration is part of Phase 2O validation.

Phase 2P adds validation for:

- local run-flow descriptor and escalation validation
- sample enemy JSON validation for swarm, miniboss, and boss definitions
- clear validation failures for invalid boss/miniboss role, timing, health, move speed, radius, contact interval, contact damage, XP, and duplicate enemy IDs
- timed miniboss and boss spawn smoke
- miniboss death and XP drop smoke
- boss death and victory trigger smoke
- survival-duration victory smoke
- existing defeat/restart flow

No package extraction, package publishing, Survivors template registration, Idle template mutation, or Movement-FPS migration is part of Phase 2P validation.

Phase 2Q adds validation for:

- runtime reward/meta descriptor validation
- sample reward JSON validation for blood shards, legacy XP, persistent upgrades, and reward definitions
- clear validation failures for duplicate currency IDs, duplicate persistent upgrade IDs, invalid persistent upgrade targets, missing persistent upgrade effects, invalid rank costs, duplicate reward IDs, missing reward currency/track references, and empty rewards
- reference-shaped run reward calculation for duration, level, miniboss kills, boss kills, victory, and boss bonus rewards
- v1-to-v2 meta profile save migration
- miniboss reward grant when a run ends in defeat
- final boss reward grant and victory reward persistence
- save/load persistence across controller instances
- persistent meta upgrade affecting a later run's projectile damage

The Phase 2Q save path uses `com.deucarian.persistence` and `com.deucarian.progression` from local Survivors kit code. Boss relic drafts, class unlocks, skill trees, richer meta upgrade graphs, and concrete-product reward economies are deliberately deferred.

No package extraction, package publishing, Survivors template registration, Idle template mutation, or Movement-FPS migration is part of Phase 2Q validation.

Phase 2R adds validation for:

- runtime relic/class descriptor validation
- sample relic JSON validation for unique IDs, valid targets, valid effect kinds, positive weights, and valid effect amounts
- sample class JSON validation for unique IDs, valid starting weapons, default unlock availability, unlock reward IDs, and valid stat modifiers
- v2-to-v3 meta profile shape through selected/unlocked class fields
- class unlock and selected class persistence through the local meta profile
- miniboss death opening a boss relic draft
- selected relic affecting the current run
- final boss victory granting the sample class unlock
- selected class affecting a new run's starting move speed, damage, and max health
- persisted class unlock selection across controller instances

The Phase 2R relic and class systems remain local Survivors template-kit code. Skill trees, class-specific upgrade pools, class resource profiles, content-pack gates, boss relic rarity tiers, and reward-selection timeout behavior are deliberately deferred.

No package extraction, package publishing, Survivors template registration, Idle template mutation, or Movement-FPS migration is part of Phase 2R validation.

Phase 2S adds validation for:

- runtime class loadout descriptor coverage
- runtime and sample validation for default class IDs, starting weapon/loadout references, and class-gated upgrade references
- clear validation failures for invalid class loadouts, missing/duplicate/empty class loadout entries, unknown default classes, unknown allowed classes, unknown gated upgrades, and duplicate class gates
- locked or missing selected class fallback to the default class
- default class starting with only the basic arcane wand loadout
- unlocked selected class starting with the expected advanced loadout and stat profile
- class-specific upgrades appearing only when the selected class is valid
- existing first-slice, weapon, payload, run-flow, reward, relic, class unlock, save/load, and persistent-upgrade tests still passing

The Phase 2S class run-start and upgrade-gate systems remain local Survivors template-kit code. Passive skill trees, class capability tags, resource profiles, content packs, class starting upgrade graphs, and richer class-specific upgrade pools are deliberately deferred.

No package extraction, package publishing, Survivors template registration, Idle template mutation, or Movement-FPS migration is part of Phase 2S validation.
