# Basic Survivors Game

Small sample for the Deucarian Survivors template.

Open `Scenes/BasicSurvivorsGame.unity` and enter Play Mode. The bootstrap object builds the arena, player, horde loop, pooled enemies, pickups, projectiles, HUD, draft UI, relic UI, meta profile service, victory, defeat, and restart flow at runtime.

- Move with WASD or arrow keys.
- The default Arcane Initiate class starts with the basic arcane wand.
- The unlockable Ember Vanguard class starts with a broader loadout, different starting stats, and access to advanced class-gated upgrades.
- Auto-attacks include projectile bolts, orbit blades, melee slashes, burst novas, a hitscan beam, grenades, traps, and mines.
- Projectile upgrades can add pierce, chain, fork, and return behavior.
- Payload upgrades can add extra payloads, bigger explosions, and wider trigger radii.
- The run escalates over time, spawns a miniboss, then spawns a final boss.
- Minibosses and bosses add blood shard and legacy XP reward bonuses to the run summary.
- Miniboss defeat opens a simple boss relic choice.
- Defeating the final boss or reaching the survival-duration clear condition triggers victory.
- Defeating the final boss unlocks the Ember Vanguard sample class.
- Victory or defeat persists the run summary to a local meta profile.
- The sample includes one persistent Arcane Legacy upgrade that increases arcane wand damage in later runs.
- The sample includes a default Arcane Initiate class, one unlockable class, class-owned loadouts, and three boss relics.
- XP gems pull into the player when close.
- Press `M` to trigger a debug magnet recall.
- Pick a level-up choice with the mouse or number keys.
- Press `R` after death or victory to restart.

First run target:

- Move through the opening horde and let the wand auto-fire.
- Collect XP gems until the level-up overlay opens, then choose with the mouse or `1`, `2`, or `3`.
- Defeat the miniboss and pick a boss relic.
- Defeat the final boss or survive until the clear condition. Victory unlocks the Ember Vanguard sample class and persists the run summary to the local meta profile.

Run `Tools > Deucarian > Templates > Survivors > Validate Content` after editing sample weapons, upgrades, enemies, rewards, relics, or classes.
