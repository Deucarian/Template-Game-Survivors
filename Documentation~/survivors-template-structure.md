# Survivors Template Structure

## Runtime Kit

`Runtime/BasicSurvivorsGame.cs` contains sample-kit tuning, stable IDs, descriptors, and the small authored default catalog:

- `SurvivorsTemplateTuning`
- `SurvivorsPacingProfile`
- `BasicSurvivorsGame`
- `SurvivorsRunState`
- `SurvivorsPickupKind`

`Runtime/SurvivorsTemplateController.cs` contains the run composition root, local actors, and local genre adapters:

- `SurvivorsTemplateController`
- `SurvivorsEnemyActor`
- `SurvivorsProjectileActor`
- `SurvivorsPickupActor`
- `SurvivorsSpawnPoseResolver`
- Human Playtest, Normal, Debug Fast, and Showcase pacing profile selection
- timed reward draft behavior for level-up and boss relic choices

`Runtime/SurvivorsRunFlow.cs` contains local Survivors run-structure logic:

- elapsed-time phase progression
- escalation multipliers for swarm pressure
- timed enemy role selection for swarm, runner, bruiser, spitter, elite, miniboss, and boss pressure
- miniboss and boss profile definitions
- scheduled miniboss/boss spawn events
- boss-defeat and survival-duration victory triggers

`Runtime/SurvivorsWeaponArchetypes.cs` contains the local Survivors weapon-kit runtimes:

- projectile auto-fire adapter
- projectile fan/spread adapter for Frost Fan-style weapons
- hitscan beam adapter
- orbit blade contact runtime
- melee slash arc runtime
- burst nova sequence runtime
- targeted burst and burst-echo hooks for Cinder Burst-style mutation routes
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
- seven sample ranked persistent upgrades for later-run damage, max health, pickup range, XP gain, and reroll capacity
- selected class and unlocked class persistence

`Runtime/SurvivorsRelicsAndClasses.cs` contains local Survivors reward/class definitions:

- boss relic definitions and deterministic relic draft selection
- simple class definitions
- class-owned starting weapon loadouts
- starting stat modifiers
- class-gated run-upgrade availability rules
- selected/unlocked class library helpers

`Runtime/SurvivorsProgressionAtlas.cs` contains local Survivors progression grouping definitions:

- compact class passive atlas descriptors
- compact weapon skill track descriptors
- atlas/track nodes that point at real run-upgrade IDs
- class-specific track metadata used by `BasicSurvivorsGame.CreateClassUpgradeGates`

`Runtime/SurvivorsContentValidation.cs` contains package-local validation for authored sample libraries and runtime catalogs:

- unique weapon and upgrade IDs
- valid weapon archetype names
- valid projectile references from projectile weapons
- valid payload timing/radius values
- valid boss/miniboss enemy roles, spawn times, and combat stats
- valid run-upgrade target references
- valid reward currency, legacy XP, persistent upgrade, rank cost, and target references
- valid relic IDs, effect kinds, targets, weights, and amounts
- valid class IDs, default class references, starting weapon/loadout references, class-gated upgrade references, unlock reward IDs, and starting stat modifiers
- valid progression track IDs, passive atlas ownership, weapon-track targets, progression node references, point costs, and max-rank promises

This is reusable Survivors template-kit code, not concrete product content.

## Sample Content

`Samples~/BasicSurvivorsGame` contains the package source for the concrete sample game. Manual branch playtesting should use the imported host scene at `Assets/Samples/com.deucarian.template.game.survivors/Basic Survivors Game/Scenes/PLAYTEST_THIS_SCENE_Survivors_Game.unity`.

- `Scenes/BasicSurvivorsGame.unity`
- `Scripts/BasicSurvivorsGameBootstrap.cs`
- `Content/DefaultEnemies/enemies.json`
- `Content/DefaultWeapons/weapons.json`
- `Content/DefaultUpgrades/upgrades.json`
- `Content/DefaultPickups/pickups.json`
- `Content/DefaultRewards/rewards.json`
- `Content/DefaultRelics/relics.json`
- `Content/DefaultClasses/classes.json`
- `Content/DefaultProgression/progression.json`

The sample includes basic swarm, runner, bruiser, ranged spitter, elite, scheduled miniboss, and final boss entries; blood shards; legacy XP; seven persistent upgrades; boss/final-boss reward definitions; six boss relics; a default class with five core weapons; deeper Frost Fan, Blood Ring, Thorn Halo, Cinder Burst, and Star Beam rank/mutation/evolution paths; draftable Star Beam and Gravity Grenade weapon unlocks; public Star Beam and Gravity Grenade rank/mutation/evolution paths; one unlockable class with an advanced loadout/stat profile, class-gated Moon Slash rank/mutation/evolution path, and class-gated Rune Trap/Aether Mine rank/mutation/evolution path; draftable XP-gain and area-scaling passives; Epic upgrade spikes; normal/elite/boss rarity-weight tables; compact passive atlases; weapon skill tracks; and class-gated upgrade metadata. These files are examples for product-owned content flipping. They are not intended to become shared package code.

## Tests

- `Tests/EditMode` covers descriptors, archetype config, expanded enemy profiles, barrier/status behavior, content validation, reward/meta validation, class unlock persistence, locked/missing class fallback, class loadout/gate validation, progression atlas validation, save migration, payload config validation, run-flow config validation, draft choice determinism, spawn flow, weapon death/drop, XP collection, level-up choice, archetype upgrade hooks, projectile modifier upgrade hooks, payload upgrade hooks, and magnet recall.
- `Tests/PlayMode` covers first playable runtime boot, death/restart, Human Playtest defaults, selected class loadouts, draftable weapon unlocks, atlas-derived class-gated upgrade availability, class-gated Moon Slash and placed-hazard rank/evolution eligibility, default reward choices waiting for the player, timed level-up/relic auto-pick when configured, enemy hit flashes, enemy death bursts, ranged enemy shot cues, major reward-cache beacons, rarity-card presentation and selected reward banners, Arcane Storm, Blizzard Crown, and Inferno Heart evolution behavior, explicit weapon-evolution feedback, orbit damage, melee/burst damage, hitscan damage, projectile pierce/chain/fork/return smoke, grenade payload damage, placed trap trigger damage, splitter child spawns, elite reward drafts, timed miniboss/boss spawning, boss/miniboss death, relic choice/application, victory, endless continuation, reward grants, class unlock persistence, save/load persistence, no-reset persistence behavior for play start/profile application/restart, explicit reset behavior, persistent upgrade effects, XP-gain and area-scaling passives, early/boss rarity weighting, and run upgrades affecting new archetypes/modifiers.
