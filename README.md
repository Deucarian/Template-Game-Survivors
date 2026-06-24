# Deucarian Template Game - Survivors

First playable Survivors-style template package. This phase ports the core Vampire-clone loop into a Deucarian-shaped package without migrating the full game or extracting new shared packages.

## First Slice

- Top-down player movement on the XZ plane.
- Radial enemy spawning around the player.
- Enemy chase and contact damage.
- Auto-fired projectile weapon targeting the nearest enemy.
- Local orbit, melee slash, and burst nova weapon archetypes.
- Enemy death, XP gem drops, pickup attraction, and magnet recall.
- Three-choice level-up draft powered by `com.deucarian.run-upgrades`.
- Upgrade application through local Survivors kit adapters.
- Game-over and restart flow.

## Package Stance

This template uses existing Deucarian packages where their fit is already concrete:

- `Combat` for health and damage resolution.
- `World Spawning` for pooled enemies, pickups, and projectile visuals.
- `Weapon Systems` and `Projectiles` for stable descriptors.
- `Run Upgrades` for draft and run-upgrade selection state.
- `Encounters` for authored spawn-flow descriptors.

The following remain local Survivors kit code for now: player movement, camera feel, radial spawn pose rules, XP magnet behavior, level-up HUD, concrete projectile behavior, orbit motion, melee arc overlap, and burst nova timing.
