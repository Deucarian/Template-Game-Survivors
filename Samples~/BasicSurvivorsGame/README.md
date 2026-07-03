# Basic Survivors Game

Playable sample for the Deucarian Survivors template.

For branch playtesting, open `C:\Repositories\Template-Game-Survivors-Playtest` and then open `Assets/Samples/com.deucarian.template.game.survivors/Basic Survivors Game/Scenes/PLAYTEST_THIS_SCENE_Survivors_Game.unity`. The root scene hierarchy includes `PLAYTEST_THIS_SCENE_OPEN_ME`. The bootstrap object builds the arena, player, horde loop, pooled enemies, pickups, projectiles, HUD, draft UI, relic UI, meta profile service, victory, defeat, and restart flow at runtime.

- Move with WASD or arrow keys.
- The sample defaults to `SurvivorsPacingProfile.HumanPlaytest`: `Time.timeScale` stays at `1`, opening spawns use a `3.35` second interval, the opening max alive cap is `10`, and reward choices stay open until the player chooses.
- The default Arcane Initiate class starts with Arc Bolt, Frost Fan, Blood Ring, Thorn Halo, and Cinder Burst so the first minute already feels like a real Survivors run.
- Level-up drafts can unlock Star Beam or Gravity Grenade when weapon slots are open; once Star Beam is owned, Star Focus ranks open Prismatic Beam, Star Pulse, and Tempest Prism build goals, while Gravity Grenade can rank Extra Payload, branch into Bigger Booms or Wider Triggers, and evolve through Giant Rune.
- Default weapons have deeper paths too: Arc Bolt evolves into Arcane Storm with a radial bolt ring; Frost Fan ranks into Frost Splinter, Frost Ricochet, and Blizzard Crown; Blood Ring and Thorn Halo rank into Thorn Halo Wall, Halo Spiral, and Crimson Aegis; and Cinder Burst ranks into Cinder Echoes, Targeted Burst Sigils, and Inferno Heart.
- The unlockable Ember Vanguard class starts with a broader loadout, different starting stats, and access to advanced class-gated upgrades, including a Moon Slash path from Moonlit Edge into Crescent Chain, Lunar Tempo, and Eclipse Waltz plus a Rune Trap/Aether Mine path from Rune Lattice into Snaring Runes, Aether Bloom, and Aetherfield Matrix.
- Each sample class owns a compact passive atlas, and the core weapons expose compact skill tracks that group rank and mutation upgrades.
- Auto-attacks include projectile bolts, orbit blades, melee slashes, burst novas, a hitscan beam, grenades, traps, and mines.
- Resolved enemy and player damage appears as short-lived numbers, and low player health adds a red screen-edge warning.
- Projectile upgrades can add fan spread, pierce, chain, fork, and return behavior.
- Status and sustain upgrades can add poison, bleed, execute, lifesteal, barrier capacity, barrier recovery, and barrier-on-damage loops.
- Enemies escalate from basic swarm pressure into fast runners, bruisers, ranged spitters, splitters that burst into smaller enemies, Blood Warden and Dread Lantern elite variants, minibosses, and a final boss.
- Payload upgrades can add extra payloads, bigger explosions, wider trigger radii, and denser placed hazard fields.
- The run escalates over a 30-minute arc, spawns a miniboss around the 7-minute mark in Human Playtest, then pushes toward a final boss at the 20-minute mark and a survival clear at 30 minutes.
- Elites, minibosses, and bosses add blood shard and legacy XP reward bonuses to the run summary.
- Elite and miniboss defeat open stronger upgrade reward drafts.
- Boss defeat strongly prefers an eligible legendary weapon evolution; if no evolution is ready, it falls back to a stronger rare-or-better reward draft before resolving victory.
- Human Playtest level-up, elite reward, boss evolution, and boss relic choices do not auto-pick; faster validation profiles can use timed auto-pick.
- Defeating the final boss or reaching the survival-duration clear condition triggers victory.
- After victory, press `C` or the Continue button to keep playing in endless escalation with denser pressure.
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
- Press `C` after victory to continue into endless escalation.
- Press `R` after death or victory to restart.
- Use `Tools > Deucarian > Templates > Survivors > Runtime Debugger` during Play Mode to force XP, grant blood shards, force level-up, force elite/miniboss/boss spawns, spawn bursts, fill the arena, switch pacing profiles with a current-run restart, apply stress targets, trigger magnet recall, inspect live build ranks, eligible evolutions, current drafts, or explicitly reset save/progress.

First run target:

- Move through the opening horde and let the wand auto-fire.
- Collect XP gems until the level-up overlay opens, then choose with the mouse or `1`, `2`, or `3`.
- Pick Star Beam when it appears, rank Star Focus five times, branch into Prismatic Beam or Star Pulse, and take Twin Charm to build toward Tempest Prism; or pick Gravity Grenade, rank Extra Payload and Bigger Booms, then take Giant Rune to build toward Gravefield Engine.
- For a default-class evolution, rank Arcane Damage five times and take Arcane Thesis to build Arc Bolt toward Arcane Storm, rank Frost Fan five times and take Frost Needlework, rank Orbiting Focus five times and take Blood Ring Canticle, or rank Nova Echo five times and take Cinder Script.
- After unlocking Ember Vanguard, rank Moonlit Edge five times and take Moon Oath to build Moon Slash toward Eclipse Waltz.
- For the Ember Vanguard hazard build, rank Rune Lattice five times and take Siege Payloads to build Rune Trap and Aether Mine toward Aetherfield Matrix.
- Defeat the miniboss and pick an elite upgrade reward.
- Build toward an evolution by ranking a weapon path and taking the matching passive.
- Defeat the final boss; if an evolution is eligible, pick the boss evolution reward, otherwise pick or skip the strong fallback reward. Victory unlocks the Ember Vanguard sample class and persists the run summary to the local meta profile, then `C` continues the same build into endless escalation.

For manual timing checks, use the host scene path above rather than the package source scene.

Run `Tools > Deucarian > Templates > Survivors > Validate Content` after editing sample weapons, upgrades, enemies, rewards, relics, classes, or progression tracks.
