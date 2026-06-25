# Deucarian Template Game - Survivors

First playable Survivors-style template package. This phase ports the core Vampire-clone loop into a Deucarian-shaped package without migrating the full game or extracting new shared packages.

## First Slice

- Top-down player movement on the XZ plane.
- Radial enemy spawning around the player.
- Enemy chase and contact damage.
- Auto-fired projectile weapon targeting the nearest enemy.
- Local projectile, orbit, melee slash, burst nova, hitscan/beam, grenade, trap, and mine weapon archetypes.
- Local projectile modifiers for pierce, chain, fork/split, and return/boomerang behavior.
- Local payload behavior for arcing grenades, placed traps/mines, delayed arming, proximity triggers, area explosions, and simple hazard ticks.
- Local run timer, escalation, miniboss, boss, victory, and defeat/restart flow.
- Local boss/miniboss reward bonuses, run result summary, blood shard currency, legacy XP, and one persistent damage upgrade.
- Local boss relic reward draft choices that affect the current run.
- Local class definitions, selected class persistence, class-owned starting loadouts/stats, class-gated upgrade availability, and one unlockable sample class.
- Local persisted meta profile with schema migration support, unlock state, selected class state, and reset/debug hooks for validation.
- Enemy death, XP gem drops, pickup attraction, and magnet recall.
- Three-choice level-up draft powered by `com.deucarian.run-upgrades`.
- Upgrade application through local Survivors kit adapters.
- Authored content validation for IDs, archetype references, projectile references, upgrade targets, and boss/miniboss enemy definitions.
- Authored content validation for relic IDs/effects/targets and class IDs/starting weapons/unlock requirements.
- Victory, game-over, and restart flow.

## Package Stance

This template uses existing Deucarian packages where their fit is already concrete:

- `Combat` for health and damage resolution.
- `World Spawning` for pooled enemies, pickups, and projectile visuals.
- `Weapon Systems` and `Projectiles` for stable descriptors.
- `Run Upgrades` for draft and run-upgrade selection state.
- `Encounters` for authored spawn-flow descriptors.
- `Progression` for local meta currency, legacy XP, and ranked persistent upgrade state.
- `Persistence` for the local Survivors meta profile save document.

The following remain local Survivors kit code for now: player movement, camera feel, radial spawn pose rules, run timing/escalation, boss/miniboss scheduling, victory state, boss reward rules, boss relic choice rules, class selection/unlock rules, class starting loadouts, class starting stat modifiers, class upgrade gates, run summary data, meta upgrade effects, XP magnet behavior, level-up/relic HUD, concrete projectile behavior, hitscan targeting/beam visuals, projectile modifier rules, payload placement/detonation/hazard rules, orbit motion, melee arc overlap, and burst nova timing.

No shared package extraction or package publishing happens in this phase.
