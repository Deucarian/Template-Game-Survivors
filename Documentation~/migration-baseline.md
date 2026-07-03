# Survivors Migration Baseline

Reference: the existing local Vampire Survivors clone repository used for Phase 2L baseline capture.

## Observed Reference Behavior

- Boot flow uses `Bootstrap`, `MainMenu`, and `Gameplay` scenes.
- Gameplay is a top-down XZ arena loop with keyboard movement.
- Player auto-attacks nearest enemies with class-owned weapon loadouts.
- Projectile, orbit, burst, melee, hitscan, grenade, trap, mine, summon, and payload archetypes exist in the full clone.
- Orbit weapons maintain rotating contact blades with per-enemy hit cooldowns.
- Melee weapons anchor toward a nearby enemy, apply an arc overlap, cap targets by hit count, and show a short slash visual.
- Burst weapons queue one or more nova pulses, optionally repeated by authored timing and later mutation hooks.
- Hitscan/beam weapons exist in the reference as nearest-target beams with optional piercing, but are not part of the Phase 2M playable slice.
- Enemies spawn from authored waves, chase the player, and apply contact damage on cooldown.
- Enemies drop XP pickups on death.
- XP pickups attract inside pickup range, then accelerate into the player.
- Magnet pickups recall all active XP gems with an escalating vacuum effect.
- Level-up pauses the run into a three-choice draft.
- Choosing a draft item applies a run upgrade and resumes play.
- Player death transitions to game over and restart is supported.
- Boss/final-boss encounters spawn from authored schedules, recurring bosses can grant bonus rewards and boss relic choices, final boss death triggers victory, and run escalation increases pressure over elapsed time.
- Class selection, passive trees, skill graphs, save migration, meta progression, and stress tooling exist in the reference. The template now ports compact local versions of class selection, meta progression, stress tooling, class passive atlases, and weapon skill tracks while leaving the reference product's full graph/editor ecosystem out of scope.

## Phase 2L Slice

The first Deucarian Survivors template slice ports only:

- player movement
- radial enemy spawning
- enemy chase and contact damage
- auto projectile weapon
- enemy death
- XP drop and collection
- magnet recall
- level-up draft and upgrade application
- game-over and restart

Everything else remains documented for later parity phases.

## Phase 2M Weapon Archetype Slice

The second slice adds local template-kit implementations for:

- orbit contact blades
- melee slash arcs
- burst nova pulses
- run-upgrade hooks for orbit blade count, melee target count, and burst count

These stay local to the Survivors template. No shared package extraction is performed in this phase.

## Phase 2N Hitscan And Modifier Slice

The reference clone's hitscan runtime fires short-lived nearest-target beams. It can pierce along a beam line when the skill snapshot or global projectile pierce stats enable it. Its projectile runtime tracks already-hit enemies and supports local counters for pierce, chain retargeting, fork/split projectiles, and return/boomerang movement. Grenade, trap, and mine payloads also exist in the reference, but they are larger payload systems and are documented only for this phase.

The 2N template slice adds local template-kit implementations for:

- hitscan/beam weapon behavior
- projectile pierce counters
- projectile chain retargeting to nearby unhit enemies
- projectile fork/split spawns toward nearby unhit enemies
- projectile return/boomerang movement after hit or expiry
- run-upgrade hooks for the new projectile modifiers and beam piercing
- authored content validation for sample weapon and upgrade libraries

These systems remain local to the Survivors template. No shared package extraction, publishing, or package registry work is performed in this phase.

## Phase 2O Payload And Placed Weapon Slice

The reference clone's grenade runtime throws arcing payloads toward a nearby target, waits for travel and fuse timing, then applies area damage. The full reference also supports bounce, clusters, chain-reaction bursts, and optional ground hazards. Its placed payload runtime deploys traps and mines around a target or the player, arms them after a delay, triggers on enemy proximity, respects lifetime expiry, and lets mines auto-detonate when they expire.

The 2O template slice adds local template-kit implementations for:

- grenade-style thrown payloads
- trap-style placed payloads with arming, lifetime, and proximity trigger
- mine-style placed payloads that can auto-detonate on expiry
- area explosion overlap and primitive pulse visuals
- simple local hazard zones with tick damage
- run-upgrade hooks for extra payloads, explosion radius, and trigger radius
- payload content validation for timing and radius fields

The richer reference behavior for grenade bounce, cluster payloads, payload chain reactions, slip hazards, and damage augment propagation is deliberately not ported yet. These systems remain local to the Survivors template. No shared package extraction, publishing, or package registry work is performed in this phase.

## Phase 2P Boss, Miniboss, And Victory Slice

The reference clone schedules bosses through wave/boss cadence data. Regular boss encounters appear before the final boss, boss deaths can grant bonus rewards and open boss-relic choices, final boss death transitions to victory, and a run escalation model increases spawn pressure and enemy/reward multipliers over time.

The 2P template slice adds local template-kit implementations for:

- run timer and phase progression
- swarm escalation for spawn interval, max alive, health, speed, and XP
- scheduled miniboss spawn
- scheduled final boss spawn
- boss/miniboss profile sample content
- boss/miniboss death counters and XP drops
- final boss death victory
- survival-duration victory
- clearer defeat/restart and victory/restart state handling
- boss/miniboss content validation

The reference boss relic draft, blood shard rewards, legacy XP rewards, authored multi-boss schedules, elites, death-spawn enemies, meta-progression grant flow, and stress tooling are deliberately not ported yet. These systems remain local to the Survivors template until another concrete game proves a reusable package shape. No shared package extraction, publishing, or package registry work is performed in this phase.

## Phase 2Q Rewards, Meta Progression, And Save Slice

The reference clone grants run-end blood shards and legacy XP from elapsed duration, level reached, elite/miniboss kills, boss kills, victory, and boss encounter bonuses. It persists a profile with lifetime and unspent blood shards, legacy XP, best run stats, boss victories, selected content, unlocks, and ranked meta upgrades. It also includes richer boss relic choices, class unlocks, skill trees, and save migration history.

The 2Q template slice adds local template-kit implementations for:

- run result summary data
- blood shard and legacy XP rewards using the reference reward-calculation shape
- miniboss and final-boss reward bonuses
- persisted meta profile with lifetime/unspent blood shards, legacy XP, best run data, completed runs, boss victories, and ranked persistent upgrade records
- v1-to-v2 meta profile migration
- sample persistent upgrades that increase later-run damage, max health, pickup range, XP gain, and reroll capacity
- sample reward/meta JSON content and validation
- reset/debug persistence hooks for tests and future editor tooling

Boss relic drafts, class unlocks, skill trees, selected class/level persistence, concrete-game economy tuning, and full reference save-schema parity are deliberately deferred. These systems remain local to the Survivors template until concrete reuse is proven. No shared package extraction, publishing, or package registry work is performed in this phase.

## Phase 2R Relic Rewards, Classes, And Meta Content Slice

The reference clone separates normal level-up upgrades from boss relic rewards. Miniboss/boss reward selections use the same paused choice surface but draw from a boss relic pool and do not consume pending level-up selections. The reference also resolves a selected class at run start, applies class starting weapons/stats/upgrades, persists selected class state, and supports unlockable classes with richer passive trees and content-pack/resource-profile gates.

The 2R template slice adds local template-kit implementations for:

- boss relic reward drafts after miniboss defeat
- selecting one relic reward and applying its current-run effect
- sample relic effects for damage, fire-rate cooldown multiplier, and pickup range
- simple class definitions
- selected class persistence in the local meta profile
- default class unlock persistence
- one unlockable sample class granted by final boss victory
- selected class starting stat modifiers for move speed, damage, and max health
- v2-to-v3 meta profile migration for selected/unlocked class IDs
- sample relic/class JSON content and validation

The richer reference behavior for passive skill trees, class-specific upgrade pools, class resource profiles, content-pack gates, class starting upgrade graphs, boss relic rarity tiers, reward selection timeout, and product-specific economy tuning is deliberately not ported yet. These systems remain local to the Survivors template until concrete reuse is proven. No shared package extraction, publishing, or package registry work is performed in this phase.

## Phase 2S Class Run-Start And Upgrade Gate Slice

The reference clone resolves the selected player class at run start, applies class-owned starting stats, grants class-owned starting weapons before fallback weapons, and filters run-upgrade availability by class ownership/capability gates. It also persists selected/unlocked class state and falls back safely when the saved selection is no longer valid.

The 2S template slice adds local template-kit implementations for:

- selected class controlling the run-start weapon loadout
- selected class controlling starting move speed, damage, and max health
- default Arcane Initiate fallback when a saved selected class is locked or missing
- persisted unlock/selection behavior for the Ember Vanguard sample class
- class-gated advanced upgrade availability for Ember-only passive tools, with public draft-owned weapon paths where the default class can unlock them
- sample JSON metadata for default class, class loadouts, and allowed upgrade classes
- content validation for class loadouts, default class references, and class-gated upgrade references

The richer reference behavior for authored capability tags, class resource profiles, archetype/signature content packs, class starting upgrade graphs, and class-specific upgrade pools beyond simple allowed-class gates is deliberately deferred in this phase. These systems remain local to the Survivors template. No shared package extraction, publishing, or package registry work is performed in this phase.

## Phase 3G Compact Progression Atlas Slice

The reference clone gives each class a passive atlas and gives weapons authored skill tracks. The full product version uses a larger authoring and graph workflow, but the useful template-sized behavior is that class and weapon upgrade routes are grouped, validated, and used to shape class-specific upgrade availability.

The 3G template slice adds local template-kit implementations for:

- compact passive atlas and weapon skill track descriptors
- sample progression JSON under `Content/DefaultProgression`
- class-specific upgrade gates derived from class-owned progression tracks
- sample Arcane and Ember passive upgrade groups
- core weapon skill track groups for Arc Bolt, Frost Fan, Blood Ring, Thorn Halo, and Cinder Burst
- validation for track IDs, class references, weapon references, upgrade references, node costs, node kinds, and max-rank promises

The full reference passive graph UI, per-weapon graph editor, twelve-class content-pack ecosystem, element/resistance matrix, class resource economies, and production authoring workflow remain deliberately unported.

## Phase 3H Reward Selection Timeout Slice

The reference clone keeps long sessions moving by automatically resolving an open reward choice after a timeout. The template now applies the same behavior to both normal level-up drafts and boss relic drafts.

The 3H template slice adds local template-kit implementations for:

- reward-selection timeout tuning
- countdown ticking while the run is paused in the reward state
- first-choice auto-pick for level-up drafts
- first-choice auto-pick for boss relic drafts
- PlayMode coverage for both timed reward surfaces

The timeout remains sample UI flow in `SurvivorsTemplateController`. No production UI framework or shared reward-selection package is introduced.
