# Basic Survivors Game

Playable sample for the Deucarian Survivors template.

Open `Scenes/BasicSurvivorsGame.unity` and enter Play Mode. The bootstrap object builds the arena, player, horde loop, pooled enemies, pickups, projectiles, HUD, draft UI, relic UI, meta profile service, victory, defeat, and restart flow at runtime.

- Move with WASD or arrow keys.
- The default Arcane Initiate class starts with Arc Bolt, Frost Fan, Blood Ring, Thorn Halo, and Cinder Burst so the first minute already feels like a real Survivors run.
- The unlockable Ember Vanguard class starts with a broader loadout, different starting stats, and access to advanced class-gated upgrades.
- Each sample class owns a compact passive atlas, and the core weapons expose compact skill tracks that group rank and mutation upgrades.
- Auto-attacks include projectile bolts, orbit blades, melee slashes, burst novas, a hitscan beam, grenades, traps, and mines.
- Projectile upgrades can add fan spread, pierce, chain, fork, and return behavior.
- Status and sustain upgrades can add poison, bleed, execute, lifesteal, barrier capacity, barrier recovery, and barrier-on-damage loops.
- Enemies escalate from basic swarm pressure into fast runners, bruisers, ranged spitters, elites, minibosses, and a final boss.
- Payload upgrades can add extra payloads, bigger explosions, and wider trigger radii.
- The run escalates over a 30-minute arc, spawns a miniboss around the 5-minute mark, then pushes toward a final boss at the 20-minute mark and a survival clear at 30 minutes.
- Minibosses and bosses add blood shard and legacy XP reward bonuses to the run summary.
- Miniboss defeat opens a simple boss relic choice.
- Level-up and boss relic choices auto-pick the first option if their countdown expires.
- Defeating the final boss or reaching the survival-duration clear condition triggers victory.
- Defeating the final boss unlocks the Ember Vanguard sample class.
- Victory or defeat persists the run summary to a local meta profile.
- The sample includes three persistent meta upgrades that increase later-run damage.
- The sample includes a default Arcane Initiate class, one unlockable class, class-owned loadouts, passive atlases, weapon skill tracks, and six boss relics.
- XP gems pull into the player when close.
- Press `M` to trigger a debug magnet recall.
- Pick a level-up choice with the mouse or number keys.
- Press `R` after death or victory to restart.
- Use `Tools > Deucarian > Templates > Survivors > Runtime Debugger` during Play Mode to force XP, force level-up, spawn bursts, fill the arena, apply stress targets, trigger magnet recall, inspect live stats, or reset meta progression.

First run target:

- Move through the opening horde and let the wand auto-fire.
- Collect XP gems until the level-up overlay opens, then choose with the mouse or `1`, `2`, or `3`.
- Defeat the miniboss and pick a boss relic.
- Defeat the final boss or survive until the clear condition. Victory unlocks the Ember Vanguard sample class and persists the run summary to the local meta profile.

Run `Tools > Deucarian > Templates > Survivors > Validate Content` after editing sample weapons, upgrades, enemies, rewards, relics, classes, or progression tracks.
