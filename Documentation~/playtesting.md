# Survivors Playtesting

Use the clear local host project for human playtesting:

`C:\Repositories\Template-Game-Survivors-Playtest`

The host references this package directly:

`com.deucarian.template.game.survivors: file:C:/Repositories/Template-Game-Survivors`

Open this imported/editable scene:

`Assets/Samples/com.deucarian.template.game.survivors/Basic Survivors Game/Scenes/PLAYTEST_THIS_SCENE_Survivors_Game.unity`

Press Play from that scene. The root scene hierarchy includes `PLAYTEST_THIS_SCENE_OPEN_ME`, and the sample should start immediately with no setup menu or repair step.

## Default Profile

The sample defaults to `SurvivorsPacingProfile.HumanPlaytest`.

Human Playtest uses `Time.timeScale == 1`, an enemy spawn interval of `1.15` seconds, an opening maximum of `34` alive enemies, basic enemy speed of `1.35` versus player speed of `5.4`, a short Arc Step dash on Space for emergency spacing, projectile speed of `8.5`, pickup attract range of `2.5`, center-screen horde-rush and major-threat warnings, and no reward-choice auto-pick timeout.

Escalation is readable but active: max alive rises by `8` about every `45` seconds, spawn interval drops by `0.08` seconds per escalation, runners can begin after about `35` seconds, bruisers after about `90` seconds, spitters after about `150` seconds, horde-rush rings begin before the first elite and pay out when cleared, the first elite arrives around `180` seconds, the dread elite arrives around `300` seconds, and the miniboss waits until `420` seconds.

## What To Check

First 30 seconds:

- The scene starts in Human Playtest, not Debug Fast.
- Basic swarm enemies enter slowly enough to read movement, dodge spacing, pickup gems, and the starting weapon kit.
- Nearby enemies should keep slight readable spacing while pressing inward instead of collapsing into one stacked blob.
- Arc Step moves the player out of a crowded pocket, briefly prevents damage, and shoves nearby enemies without becoming a permanent speed boost.
- Fast projectile shots should still damage enemies they visibly cross, even during single-frame speed spikes.
- XP gems should pulse while being pulled in; rapid clusters should show a Gem Rush banner and briefly improve damage, cooldown, movement, and pickup attraction, pickup-range upgrades should affect gems already on the ground, and magnet pickups, evolution selections, or `M` should make distant XP gems spin and surge toward the player.
- Sustained movement through the endless arena should drop small roaming XP caches; longer travel should eventually add magnet recalls, shard bonuses, and small trailing ambushes to some caches, and clearing an ambush pack should drop a small XP burst.
- Sustained kill streaks should briefly announce bonus XP, vital-shard, magnet, and blood-shard drops when they trigger.
- At longer streak milestones, Tempo Surge should appear and briefly make weapons hit harder, cool down faster, move the player faster, and pull new pickups from farther away.
- When wounded, sustained kill streaks or major-enemy kills can leave vital shards; collecting one should restore health and clear the low-health warning if enough health returns.
- The first damaging hit that crosses into low health should fire a Clutch Pulse, damage nearby regular enemies, leave elites and bosses intact, and briefly ignore immediate follow-up damage.
- Longer kill streaks can leave blood shards; collecting one should increase the bonus blood shards counted in the run summary.
- Opening spawns should arrive as small packs, while runners should not appear until about 35 seconds.
- The first level-up draft should appear within about 30-60 seconds once the player moves through XP gems.
- Draft cards should show rarity, category, affected build piece, description, and current-to-next rank such as `Rank 1->2/5`.
- Selecting a normal level-up card should flash a small Level Pulse that hits nearby regular enemies without deleting elite pressure.
- Filling the weapon slots should flash an Arsenal Surge banner, hit nearby regular enemies, leave major threats intact, and briefly improve damage, cooldown, movement, and pickup attraction.
- Selecting a second evolved weapon should flash a Legend Surge banner, hit nearby regular enemies, leave major threats intact, and briefly improve damage, cooldown, movement, and pickup attraction.
- If no valid upgrade choices are available, the run should automatically use the skip fallback, grant the small shard reward, and resume instead of leaving an empty choice overlay open.
- No console errors, missing scripts, missing materials, or missing package references should appear.

After 2 minutes:

- The run should have opened at least one readable level-up choice, and most runs should have resolved multiple draft picks if the player keeps collecting gems.
- Runners and bruisers should be present, but the screen should still be understandable.
- Rewards should stay open until you choose with mouse, `1`, `2`, or `3`, then briefly show a selected-reward banner after the overlay closes.
- Restart after defeat should still work with `R` or the restart button.

After 5 minutes:

- Spitters should have joined the enemy mix, and their ranged attacks should show quick hostile shot cues before damage lands; moving out of range during the cue should avoid the hit.
- The first elite should have arrived around 3 minutes and the dread elite should be arriving around 5 minutes; active major threats should show a prioritized health bar, dread elite slam ground discs should warn before any area damage, and killing either elite should open an elite reward draft and scatter a visible XP/special-pickup reward cache that pulls inward after the reward choice resolves.
- Each scheduled elite should show a short incoming warning before it appears.
- Damaging an elite below its low-health threshold should trigger one visible support call-in, then further damage to that same elite should not repeatedly spawn extra rings.
- At least one horde rush should have warned, spawned a mixed enemy ring around the player, briefly announced the rush on the HUD, and dropped a bonus cache after the tracked rush enemies were cleared.
- The game should feel busier than the first minute without becoming immediate visual noise.

After 7 minutes:

- The miniboss should have appeared, shown a health bar while alive, opened an elite reward draft on death, then chained into a boss relic choice before play resumes; its death should burst into a larger reward cache whose drops pull inward when the reward sequence resolves.
- The boss relic choice should not include relics already selected earlier in the same run; if every relic has been claimed, the reward sequence should close and resume cleanly.
- Selecting a boss relic should emit a brief Relic Surge that damages nearby non-major enemies without deleting the major-threat reward flow, briefly improves damage, cooldown, movement, and pickup attraction, and shows a Relic timer in the HUD.
- After selecting a relic, the build HUD should update its relic count, and the eventual victory or defeat summary should include relics alongside weapons, passives, and evolutions.
- The miniboss warning should have appeared shortly before the miniboss entered.
- The miniboss should paint a slam warning disc before area damage and call in a larger support ring near low health, making the fight briefly spike without hiding the health bar or reward flow.
- The run should have a clear build direction from weapon ranks, passives, or branch mutations.
- If a weapon path has reached its required rank and the matching passive is owned, the HUD should briefly announce Evolution Ready for that payoff.
- Selecting an evolution should immediately fire a surge that hits nearby non-major enemies, then the evolved weapon behavior should be visible on subsequent attacks.

After victory:

- Press `C` or the Continue button to stay in the same build.
- The victory panel should summarize time, level, kills, build size, run rewards, meta totals, any class unlock, next-run class choices, and affordable meta upgrades before you continue or restart.
- If Ember Vanguard has just unlocked, select it from the result panel, restart, and confirm the next run starts with the broader Ember loadout and class-gated upgrade pool.
- Endless escalation should keep increasing horde pressure and should schedule recurring elite, miniboss, and boss threats with incoming warnings.
- Killing recurring major threats should keep producing the same stronger reward drafts and arena pickup caches without ending the run again, and each new recurring major threat can still trigger its own one-shot support call-in.

After defeat:

- The defeat panel should summarize the same run rewards, meta totals, next-run class choices, and affordable meta upgrades, then `R` or the Restart button should begin a fresh run and clear the previous summary.

## Reset Save Data

Enter Play Mode, then open:

`Tools > Deucarian > Templates > Survivors > Runtime Debugger`

Click `Explicitly Reset Save / Progress`. Restart Play Mode to confirm the sample starts from a fresh local meta profile. The same window can grant blood shards for meta-upgrade checks and inspect the current build ranks, eligible evolutions, and current draft pool. Entering Play Mode, restarting the current run, and applying a pacing profile should not wipe saved meta progress.

## Enable Debug Fast

Debug Fast is opt-in only.

1. Enter Play Mode in the scene above.
2. Open `Tools > Deucarian > Templates > Survivors > Runtime Debugger`.
3. Set `Pacing Profile` to `DebugFast`.
4. Click `Apply Pacing Profile And Restart Current Run`.

Debug Fast uses faster spawns, faster escalation, quicker level-ups, shorter reward timeouts, and earlier boss timings for automated or quick validation. Do not use it for judging the default human-readable sample feel.
