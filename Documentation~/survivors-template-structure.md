# Survivors Template Structure

## Runtime Kit

`Runtime/BasicSurvivorsGame.cs` contains sample-kit tuning, stable IDs, descriptors, and the small authored default catalog:

- `SurvivorsTemplateTuning`
- `BasicSurvivorsGame`
- `SurvivorsRunState`
- `SurvivorsPickupKind`

`Runtime/SurvivorsTemplateController.cs` contains the run composition root, local actors, and local genre adapters:

- `SurvivorsTemplateController`
- `SurvivorsEnemyActor`
- `SurvivorsProjectileActor`
- `SurvivorsPickupActor`
- `SurvivorsSpawnPoseResolver`

`Runtime/SurvivorsRunFlow.cs` contains local Survivors run-structure logic:

- elapsed-time phase progression
- escalation multipliers for swarm pressure
- miniboss and boss profile definitions
- scheduled miniboss/boss spawn events
- boss-defeat and survival-duration victory triggers

`Runtime/SurvivorsWeaponArchetypes.cs` contains the local Survivors weapon-kit runtimes:

- projectile auto-fire adapter
- hitscan beam adapter
- orbit blade contact runtime
- melee slash arc runtime
- burst nova sequence runtime
- local projectile modifier behavior for pierce, chain, fork/split, and return/boomerang
- short-lived primitive visuals for sample-only feedback

`Runtime/SurvivorsPayloadWeapons.cs` contains the local Survivors payload-kit runtimes:

- arcing grenade payloads with fuse timing
- placed trap and mine payloads with arming, lifetime, and proximity trigger logic
- area explosion overlap and primitive explosion visuals
- simple local hazard zones with tick damage

`Runtime/SurvivorsMetaProgression.cs` contains the local Survivors reward and meta-progression adapter:

- run result summary data
- reference-shaped blood shard and legacy XP reward calculation
- boss/miniboss reward bonus definitions
- persisted meta profile document and v1-to-v3 migration
- one sample ranked persistent upgrade that adds arcane wand damage to later runs
- selected class and unlocked class persistence

`Runtime/SurvivorsRelicsAndClasses.cs` contains local Survivors reward/class definitions:

- boss relic definitions and deterministic relic draft selection
- simple class definitions
- starting stat modifiers
- selected/unlocked class library helpers

`Runtime/SurvivorsContentValidation.cs` contains package-local validation for authored sample libraries and runtime catalogs:

- unique weapon and upgrade IDs
- valid weapon archetype names
- valid projectile references from projectile weapons
- valid payload timing/radius values
- valid boss/miniboss enemy roles, spawn times, and combat stats
- valid run-upgrade target references
- valid reward currency, legacy XP, persistent upgrade, rank cost, and target references
- valid relic IDs, effect kinds, targets, weights, and amounts
- valid class IDs, starting weapon references, unlock reward IDs, and starting stat modifiers

This is reusable Survivors template-kit code, not concrete product content.

## Sample Content

`Samples~/BasicSurvivorsGame` contains a tiny concrete sample game:

- `Scenes/BasicSurvivorsGame.unity`
- `Scripts/BasicSurvivorsGameBootstrap.cs`
- `Content/DefaultEnemies/enemies.json`
- `Content/DefaultWeapons/weapons.json`
- `Content/DefaultUpgrades/upgrades.json`
- `Content/DefaultPickups/pickups.json`
- `Content/DefaultRewards/rewards.json`
- `Content/DefaultRelics/relics.json`
- `Content/DefaultClasses/classes.json`

The sample includes a swarm enemy, one scheduled miniboss, one final boss, blood shards, legacy XP, one persistent upgrade, boss/final-boss reward definitions, three boss relics, a default class, and one unlockable class. These files are examples for product-owned content flipping. They are not intended to become shared package code.

## Tests

- `Tests/EditMode` covers descriptors, archetype config, content validation, reward/meta validation, class unlock persistence, save migration, payload config validation, run-flow config validation, draft choice determinism, spawn flow, weapon death/drop, XP collection, level-up choice, archetype upgrade hooks, projectile modifier upgrade hooks, payload upgrade hooks, and magnet recall.
- `Tests/PlayMode` covers first playable runtime boot, death/restart, orbit damage, melee/burst damage, hitscan damage, projectile pierce/chain/fork/return smoke, grenade payload damage, placed trap trigger damage, timed miniboss/boss spawning, boss/miniboss death, relic choice/application, victory, reward grants, class unlock persistence, save/load persistence, persistent upgrade effects, and run upgrades affecting new archetypes/modifiers.
