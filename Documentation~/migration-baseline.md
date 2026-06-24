# Survivors Migration Baseline

Reference: the existing local Vampire Survivors clone repository used for Phase 2L baseline capture.

## Observed Reference Behavior

- Boot flow uses `Bootstrap`, `MainMenu`, and `Gameplay` scenes.
- Gameplay is a top-down XZ arena loop with keyboard movement.
- Player auto-attacks nearest enemies with class-owned weapon loadouts.
- Projectile, orbit, burst, melee, hitscan, grenade, trap, mine, summon, and payload archetypes exist in the full clone.
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
