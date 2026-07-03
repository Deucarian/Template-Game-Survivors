# Basic Survivors Game

Playable sample for the Deucarian Survivors template.

For branch playtesting, open `C:\Repositories\Template-Game-Survivors-Playtest` and then open `Assets/Samples/com.deucarian.template.game.survivors/Basic Survivors Game/Scenes/PLAYTEST_THIS_SCENE_Survivors_Game.unity`. The root scene hierarchy includes `PLAYTEST_THIS_SCENE_OPEN_ME`. The bootstrap object builds the arena, player, horde loop, pooled enemies, pickups, projectiles, HUD, draft UI, relic UI, meta profile service, victory, defeat, and restart flow at runtime.

- Move with WASD or arrow keys.
- The sample defaults to `SurvivorsPacingProfile.HumanPlaytest`: `Time.timeScale` stays at `1`, opening spawns use a `3.35` second interval, the opening max alive cap is `10`, and reward choices stay open until the player chooses.
- The default Arcane Initiate class starts with Arc Bolt, Frost Fan, Blood Ring, Thorn Halo, and Cinder Burst so the first minute already feels like a real Survivors run.
- The unlockable Ember Vanguard class starts with a broader loadout, different starting stats, and access to advanced class-gated upgrades.
- Each sample class owns a compact passive atlas, and the core weapons expose compact skill tracks that group rank and mutation upgrades.
- Auto-attacks include projectile bolts, orbit blades, melee slashes, burst novas, a hitscan beam, grenades, traps, and mines.
- Projectile upgrades can add fan spread, pierce, chain, fork, and return behavior.
- Status and sustain upgrades can add poison, bleed, execute, lifesteal, barrier capacity, barrier recovery, and barrier-on-damage loops.
- Enemies escalate from basic swarm pressure into fast runners, bruisers, ranged spitters, elites, minibosses, and a final boss.
- Payload upgrades can add extra payloads, bigger explosions, and wider trigger radii.
- The run escalates over a 30-minute arc, spawns a miniboss around the 7-minute mark in Human Playtest, then pushes toward a final boss at the 20-minute mark and a survival clear at 30 minutes.
- Minibosses and bosses add blood shard and legacy XP reward bonuses to the run summary.
- Miniboss defeat opens a stronger elite upgrade reward draft.
- Boss defeat strongly prefers an eligible legendary weapon evolution; if no evolution is ready, it falls back to a stronger rare-or-better reward draft before resolving victory.
- Human Playtest level-up, elite reward, boss evolution, and boss relic choices do not auto-pick; faster validation profiles can use timed auto-pick.
- Defeating the final boss or reaching the survival-duration clear condition triggers victory.
- Defeating the final boss unlocks the Ember Vanguard sample class.
- Victory or defeat persists the run summary to a local meta profile.
- The sample includes three persistent meta upgrades that increase later-run damage.
- The sample includes a default Arcane Initiate class, one unlockable class, class-owned loadouts, passive atlases, weapon skill tracks, and six boss relics.
- XP gems pull into the player when close.
- Press `M` to trigger a debug magnet recall.
- Pick a level-up or reward choice with the mouse or number keys.
- Press `R` during an upgrade draft to reroll while charges remain.
- Press `S` during an upgrade draft to skip for a small blood shard bonus.
- Press `Shift+1`, `Shift+2`, or `Shift+3` during an upgrade draft to banish a choice while charges remain.
- Press `R` after death or victory to restart.
- Use `Tools > Deucarian > Templates > Survivors > Runtime Debugger` during Play Mode to force XP, force level-up, spawn bursts, fill the arena, switch pacing profiles with a current-run restart, apply stress targets, trigger magnet recall, inspect live stats, or explicitly reset save/progress.

First run target:

- Move through the opening horde and let the wand auto-fire.
- Collect XP gems until the level-up overlay opens, then choose with the mouse or `1`, `2`, or `3`.
- Defeat the miniboss and pick an elite upgrade reward.
- Build toward an evolution by ranking a weapon path and taking the matching passive.
- Defeat the final boss; if an evolution is eligible, pick the boss evolution reward, otherwise pick or skip the strong fallback reward. Victory unlocks the Ember Vanguard sample class and persists the run summary to the local meta profile.

For manual timing checks, use the host scene path above rather than the package source scene.

Run `Tools > Deucarian > Templates > Survivors > Validate Content` after editing sample weapons, upgrades, enemies, rewards, relics, classes, or progression tracks.
