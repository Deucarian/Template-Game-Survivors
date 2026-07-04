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
- timed reward draft behavior for level-up choices, miniboss reward chains, and boss relic choices
- current-to-next rank labels for live draft cards and debug draft inspection
- empty level-up draft fallback that grants the skip reward and resumes play
- full-weapon Arsenal Surge feedback, nearby non-major enemy damage, and short stat momentum
- full-passive Harmony Surge feedback, nearby non-major enemy damage, and short stat/XP momentum
- boss relic Relic Surge feedback, nearby non-major enemy damage, and short stat momentum
- post-victory Endless Surge feedback, tiered reward drops, nearby non-major enemy damage, and short stat momentum for recurring major-threat kills
- travel-triggered Arena Trials with tracked enemy rings, XP/shard clear rewards, nearby non-major enemy damage, and short stat momentum
- normal level-up Level Pulse feedback and nearby non-major enemy damage
- one-shot low-health Clutch Pulse feedback, nearby non-major enemy damage, and brief player safety
- victory/defeat summary lines shared by tests and the IMGUI result panels
- weapon-evolution surge feedback, nearby non-major enemy damage, and loose-XP recall
- multi-evolution Legend Surge feedback, nearby non-major enemy damage, and short stat momentum
- timed horde-rush events that warn the player, spawn mixed enemy rings between major threats, and drop clear-reward caches
- local enemy crowd-spacing force for readable horde presentation
- prioritized major-threat health readouts for active elites, minibosses, and bosses
- one-shot major-threat support call-ins when elites, minibosses, or bosses drop below their tuned health threshold
- ground-marked major-threat slam attacks for dread elites, minibosses, and bosses
- kill-streak reward drops and temporary Tempo Surge stat spikes

`Runtime/SurvivorsRunFlow.cs` contains local Survivors run-structure logic:

- elapsed-time phase progression
- escalation multipliers for swarm pressure
- timed enemy role selection for swarm, runner, bruiser, spitter, elite, horde-rush rings, miniboss, and boss pressure
- miniboss and boss profile definitions
- scheduled miniboss/boss spawn events
- boss-defeat and survival-duration victory triggers
- post-victory endless intervals for recurring elite, miniboss, and boss pressure

`Runtime/SurvivorsWeaponArchetypes.cs` contains the local Survivors weapon-kit runtimes:

- projectile auto-fire adapter
- projectile fan/spread adapter for Frost Fan-style weapons
- hitscan beam adapter
- orbit blade contact runtime
- melee slash arc runtime
- burst nova sequence runtime
- targeted burst and burst-echo hooks for Cinder Burst-style mutation routes
- swept projectile hit checks for fast shots crossing enemies between frames
- local projectile modifier behavior for pierce, chain, fork/split, and return/boomerang
- short-lived primitive visuals, Arc Step dash feedback, dodgeable ranged-enemy warning cues, payload hazard Trap Chain XP/pulse payoffs, waystone discovery and Waystone Chain reward beats, roaming arena reward caches with optional special drops, clearable ambush packs, Arena Trial enemy rings, major-enemy pickup caches with cache-pull attraction, and HUD banners for sample-only feedback

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
- current-run relic ownership is tracked by the controller so boss relic drafts stop repeating selected relics
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
- valid pickup IDs, display names, attraction values, and behavior descriptions
- valid run-upgrade target references
- valid reward currency, legacy XP, persistent upgrade, rank cost, and target references
- valid relic IDs, effect kinds, targets, weights, and amounts
- valid class IDs, default class references, starting weapon/loadout references, class-gated upgrade references, unlock reward IDs, and starting stat modifiers
- valid progression track IDs, passive atlas ownership, weapon-track targets, progression node references, point costs, and max-rank promises
- valid run-flow pacing values, slot limits, rarity tables, endless intervals, Endless Surge rewards, Arena Trial rewards, and ranged-shot dodge XP rewards

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
- `Content/DefaultRunFlow/run-flow.json`

The sample includes basic swarm, runner, bruiser, ranged spitter with dodgeable shot wind-up and authored dodge XP rewards, elite, scheduled miniboss, and final boss entries; XP, magnet, vital-shard, and blood-shard pickup entries with Arc Step dash tuning, Gem Rush pickup-cluster stat boosts, waystone discovery and Waystone Chain rewards, roaming arena caches, roaming cache special-drop/ambush tuning, Arena Trial risk/reward tuning, major-enemy support call-ins, major-enemy slam timing/ground telegraphs, major-enemy reward caches with cache-pull attraction, streak reward banners, Tempo Surge stat spikes, and Endless Surge post-victory major-threat rewards; blood shards; legacy XP; seven persistent upgrades; elite/miniboss/final-boss reward definitions plus a concrete first-clear class-unlock reward; six boss relics chained after miniboss upgrade rewards; a default class with five core weapons; one-rank weapon unlock definitions for every authored weapon; deeper Frost Fan, Blood Ring, Thorn Halo, Cinder Burst, and Star Beam rank/mutation/evolution paths including Serrated Orbit and Bramble Guard branches; public Star Beam and Gravity Grenade rank/mutation/evolution paths; one unlockable class with an advanced loadout/stat profile, class-gated Moon Slash rank/mutation/evolution path, and class-gated Rune Trap/Aether Mine rank/mutation/evolution path; draftable XP-gain and area-scaling passives; Epic upgrade spikes; normal/elite/boss rarity-weight tables; compact passive atlases; weapon skill tracks; and class-gated upgrade metadata. These files are examples for product-owned content flipping. They are not intended to become shared package code.

## Tests

- `Tests/EditMode` covers descriptors, archetype config, expanded enemy profiles, barrier/status behavior, content validation, pickup validation, reward/meta validation, class unlock persistence, locked/missing class fallback, class loadout/gate validation, progression atlas validation, save migration, payload config validation, run-flow config validation, draft choice determinism, spawn flow, weapon death/drop, XP collection, level-up choice, archetype upgrade hooks, projectile modifier upgrade hooks, payload upgrade hooks, and magnet recall.
- `Tests/PlayMode` covers first playable runtime boot, death/restart, Human Playtest defaults, Arc Step dash movement/shove/safety/cooldown behavior, crowded swarm spacing while maintaining player pressure, selected class loadouts, owned weapon unlock suppression, class-gated weapon unlock availability, draftable weapon unlocks, full-weapon Arsenal Surge payoff, full-passive Harmony Surge payoff, atlas-derived class-gated upgrade availability, class-gated Moon Slash and placed-hazard rank/evolution eligibility, default reward choices waiting for the player, normal level-up Level Pulse payoff, low-health Clutch Pulse safety and major-threat preservation, multi-evolution Legend Surge payoff, empty level-up draft fallback, timed level-up/relic auto-pick when configured, enemy hit flashes, enemy death bursts, ranged enemy shot cues and wind-up dodging, health pickup recovery, blood-shard pickup currency, Gem Rush pickup-cluster stat boosts, waystone discovery and Waystone Chain payoff beats, roaming arena reward caches with special-drop, ambush, clear-payoff, and Arena Trial Shrine Surge beats, horde-rush warnings, arena bursts, and clear-reward caches, major reward-cache beacons, cache-pull pickup bursts, health readouts, one-shot major-threat support call-ins, and ground-marked major-threat slams, rarity-card presentation and selected reward banners, Arcane Storm, Blizzard Crown, and Inferno Heart evolution behavior, evolution-ready feedback, evolution surge damage, evolution XP recall, and explicit weapon-evolution feedback, orbit damage, melee/burst damage, hitscan damage, projectile sweep reliability, projectile pierce/chain/fork/return smoke, grenade payload damage, placed trap trigger damage, splitter child spawns, elite reward drafts, miniboss upgrade-to-relic reward chaining, timed miniboss/boss spawning, boss/miniboss death, relic choice/application and Relic Surge payoff, victory and defeat run summaries, endless continuation with recurring major threats and Endless Surge payoff, reward grants, class unlock persistence, save/load persistence, no-reset persistence behavior for play start/profile application/restart, explicit reset behavior, persistent upgrade effects, XP-gain and area-scaling passives, early/boss rarity weighting, and run upgrades affecting new archetypes/modifiers.
