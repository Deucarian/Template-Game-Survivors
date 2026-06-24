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
- Boss/final-boss, class selection, passive trees, skill graphs, save migration, meta progression, and stress tooling exist in the reference but are deliberately outside the first template slice.

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
