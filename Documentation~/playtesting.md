# Survivors Playtesting

Use the clear local host project for human playtesting:

`C:\Repositories\Template-Game-Survivors-Playtest`

The host references this package directly:

`com.deucarian.template.game.survivors: file:C:/Repositories/Template-Game-Survivors`

Open this imported/editable scene:

`Assets/Samples/com.deucarian.template.game.survivors/Basic Survivors Game/Scenes/PLAYTEST_THIS_SCENE_Survivors_Game.unity`

Press Play from that scene. The root scene hierarchy includes `PLAYTEST_THIS_SCENE_OPEN_ME`, and the sample should open the run-mode selector without requiring a setup or repair step. Choose Standard Run with `1`/Enter for the full 30-minute Human Playtest loop, or Sprint Run with `2`/`S` for the compact 5-minute loop.

## Run Modes

Standard Run uses `SurvivorsPacingProfile.HumanPlaytest`.

Human Playtest uses `Time.timeScale == 1`, an enemy spawn interval of `0.95` seconds, an opening maximum of `38` alive enemies, basic enemy speed of `1.35` versus player speed of `5.55`, a short Arc Step dash on Space for emergency spacing, projectile speed of `9.25`, pickup attract range of `2.9`, center-screen horde-rush and major-threat warnings, offscreen markers for distant major threats, and no reward-choice auto-pick timeout.

Escalation is readable but active: max alive rises by `8` about every `45` seconds, spawn interval drops by `0.08` seconds per escalation, runners can begin after about `35` seconds, bruisers after about `90` seconds, spitters after about `150` seconds, horde-rush rings begin around `75` seconds and pay out when cleared, the first elite arrives around `135` seconds, the dread elite arrives around `255` seconds, and the miniboss waits until `360` seconds.

Sprint Run uses `SurvivorsPacingProfile.SprintRun`. It targets a 5-minute session with a `300` second victory time, a `270` second boss, a first elite around `82` seconds, horde pressure around `120` seconds, dread/miniboss pressure around `165`-`175` seconds, a Sprint-specific `260 + 54 per level` XP curve, an `18` second normal level-up draft cooldown, at most `1` queued level-up draft, `5` rerolls, `5` banishes, a `0.65` meta reward multiplier, a Sprint-only evolution rank assist, and no endless continuation after victory.

## What To Check

First 30 seconds:

- The scene opens on the run-mode selector, and Standard Run starts Human Playtest rather than Debug Fast.
- Basic swarm enemies enter slowly enough to read movement, dodge spacing, pickup gems, and the starting weapon kit.
- Nearby enemies should keep slight readable spacing while pressing inward instead of collapsing into one stacked blob.
- Arc Step moves the player out of a crowded pocket, briefly prevents damage, and shoves nearby enemies without becoming a permanent speed boost.
- Fast projectile shots should still damage enemies they visibly cross, even during single-frame speed spikes.
- XP gems should pulse while being pulled in; rapid clusters should show a Gem Rush banner and briefly improve damage, cooldown, movement, and pickup attraction, pickup-range upgrades should affect gems already on the ground, and magnet pickups, evolution selections, or `M` should make distant XP gems spin and surge toward the player.
- Gemheart, Lodestone Sigil, and Vacuum Pulse should appear as readable pickup-build draft choices over repeated drafts; picking them should widen pickup reach, speed up pulled gems, or trigger periodic recalls without opening back-to-back level-up overlays.
- Sustained movement through the endless arena should discover colored waystones and drop small roaming XP caches; repeated fresh waystones should eventually flash a Waystone Chain reward with bonus XP, a nearby regular-enemy pulse, and short combat/pickup momentum, longer travel should eventually add magnet recalls, shard bonuses, small trailing ambushes, and periodic Arena Trials, and clearing those extra packs should drop a small XP/shard burst.
- Sustained kill streaks should briefly announce bonus XP, vital-shard, magnet, and blood-shard drops when they trigger.
- At longer streak milestones, Tempo Surge should appear and briefly make weapons hit harder, cool down faster, move the player faster, and pull new pickups from farther away.
- When an Arena Trial appears, its enemy ring should be readable, should not immediately replace boss/elite rewards, and should pay a Shrine Surge with XP, shards, a nearby regular-enemy pulse, and temporary combat/pickup momentum when cleared.
- When wounded, sustained kill streaks or major-enemy kills can leave vital shards; collecting one should restore health and clear the low-health warning if enough health returns.
- The first damaging hit that crosses into low health should fire a Clutch Pulse, damage nearby regular enemies, leave elites and bosses intact, and briefly ignore immediate follow-up damage.
- Longer kill streaks can leave blood shards; collecting one should increase the bonus blood shards counted in the run summary.
- Opening spawns should arrive as small packs, while runners should not appear until about 35 seconds.
- The first level-up draft should appear within about 30-60 seconds once the player moves through XP gems.
- Draft cards should show rarity, category, affected build piece, description, and current-to-next rank such as `Rank 1->2/5`.
- Selecting a normal level-up card should flash a small Level Pulse that hits nearby regular enemies without deleting elite pressure.
- Filling the weapon slots should flash an Arsenal Surge banner, hit nearby regular enemies, leave major threats intact, and briefly improve damage, cooldown, movement, and pickup attraction.
- Filling the passive slots should flash a Harmony Surge banner, hit nearby regular enemies, leave major threats intact, briefly improve damage, cooldown, movement, pickup attraction, and XP gain, and show a Harmony timer in the HUD.
- Selecting a second evolved weapon should flash a Legend Surge banner, hit nearby regular enemies, leave major threats intact, and briefly improve damage, cooldown, movement, and pickup attraction.
- If no valid upgrade choices are available, the run should automatically use the skip fallback, grant the small shard reward, and resume instead of leaving an empty choice overlay open.
- No console errors, missing scripts, missing materials, or missing package references should appear.

After 2 minutes:

- The run should have opened at least one readable level-up choice, and most runs should have resolved multiple draft picks if the player keeps collecting gems.
- Runners and bruisers should be present, but the screen should still be understandable.
- Rewards should stay open until you choose with mouse, `1`, `2`, or `3`, then briefly show a selected-reward banner after the overlay closes.
- Restart after defeat should still work with `R` or the restart button.
- Standing still in Sprint should become dangerous by this point unless the build is already unusually strong; Human Playtest should stay readable in its first minute but should not let early/mid-run enemies remain irrelevant forever.

After 5 minutes:

- Spitters should have joined the enemy mix, and their ranged attacks should show quick hostile shot cues before damage lands; moving out of range during the cue should avoid the hit.
- The first elite should have arrived around 2.25 minutes and the dread elite should have arrived around 4.25 minutes; active major threats should show a prioritized health bar, dread elite slam ground discs should warn before any area damage, and killing either elite should open an elite reward draft and scatter a visible XP/special-pickup reward cache that pulls inward after the reward choice resolves.
- Moving far away from an active elite or boss should show an edge marker with direction and distance; the threat should keep its health, catch up or safely re-enter from offscreen, and clear its marker/life bar when killed.
- Each scheduled elite should show a short incoming warning before it appears.
- Damaging an elite below its low-health threshold should trigger one visible support call-in, then further damage to that same elite should not repeatedly spawn extra rings.
- At least one horde rush should have warned, spawned a mixed enemy ring around the player, briefly announced the rush on the HUD, and dropped a bonus cache after the tracked rush enemies were cleared.
- The game should feel busier than the first minute without becoming immediate visual noise.

After 6 minutes:

- The miniboss should have appeared, shown a health bar while alive, opened an elite reward draft on death, then chained into a boss relic choice before play resumes; its death should burst into a larger reward cache whose drops pull inward when the reward sequence resolves.
- The boss relic choice should not include relics already selected earlier in the same run; if every relic has been claimed, the reward sequence should close and resume cleanly.
- Selecting a boss relic should emit a brief Relic Surge that damages nearby non-major enemies without deleting the major-threat reward flow, briefly improves damage, cooldown, movement, and pickup attraction, and shows a Relic timer in the HUD.
- After selecting a relic, the Current Build panel should update its relic count and names, and the eventual victory or defeat summary should include relics alongside weapons, passives, and evolutions.
- The miniboss warning should have appeared shortly before the miniboss entered.
- The miniboss should paint a slam warning disc before area damage and call in a larger support ring near low health, making the fight briefly spike without hiding the health bar or reward flow.
- The run should have a clear build direction from weapon ranks, passives, or branch mutations; the right-side Current Build panel should show owned weapon paths, passive ranks, evolution status, and relics without opening developer tools.
- If a weapon path has reached its required rank and the matching passive is owned, the HUD should briefly announce Evolution Ready for that payoff.
- Selecting an evolution should immediately fire a surge that hits nearby non-major enemies, then the evolved weapon behavior should be visible on subsequent attacks.

After victory:

- Press `C` or the Continue button to stay in the same build.
- The victory panel should summarize time, level, kills, build size, run rewards, meta totals, any class unlock, next-run class choices, and affordable meta upgrades before you continue or restart.
- If Ember Vanguard has just unlocked, select it from the result panel, restart, and confirm the next run starts with the broader Ember loadout and class-gated upgrade pool.
- Endless escalation should keep increasing horde pressure and should schedule recurring elite, miniboss, and boss threats with incoming warnings.
- Defeating a recurring elite, miniboss, or boss should trigger Endless Surge with a tiered HUD timer, extra XP gems, a blood-shard drop, a nearby non-major enemy pulse, and temporary damage, cooldown, movement, and pickup-range momentum.
- Killing recurring major threats should keep producing the same stronger reward drafts and arena pickup caches without ending the run again, and each new recurring major threat can still trigger its own one-shot support call-in.

After defeat:

- The defeat panel should summarize the same run rewards, meta totals, next-run class choices, and affordable meta upgrades, then `R` or the Restart button should begin a fresh run and clear the previous summary.

## Sprint Run Checks

Choose Sprint Run from the selector or start it through the runtime debugger.

- First draft should be reachable around `25`-`40` seconds when the player keeps collecting XP.
- Level should land around `3`-`4` at 1 minute, `5`-`7` at 2 minutes, `8`-`10` at 3 minutes, `11`-`14` at 4 minutes, and `14`-`18` at 5 minutes.
- A full 5-minute Sprint should usually produce about `10`-`16` normal level-up drafts, with action time between choices.
- Standing still from the opening should usually cause death or serious danger inside `60`-`120` seconds.
- The player should usually have a second weapon, passive, or meaningful rank identity by about `45`-`60` seconds.
- First elite should arrive around `82` seconds.
- Horde pressure should spike around `120` seconds.
- Dread/miniboss pressure should arrive around `165`-`175` seconds.
- A focused build should be able to become evolution-eligible between `180` and `240` seconds without changing Standard evolution requirements.
- Boss should arrive around `270` seconds and victory should resolve around `300` seconds.
- The victory summary should say Sprint Run, apply the `0.65` reward multiplier, and show Restart without the endless Continue option.
- The runtime debugger's run metrics should show the selected mode, first draft, first elite, miniboss/dread, boss, evolution, kill, XP, and damage timings when those events happen.
- The old player-centered circle and four bar-like arena markers should not appear in normal play; waystone compass guidance and explicit enemy/rush telegraphs should carry the readable navigation and danger information instead.

## Reset Save Data

Enter Play Mode, then open:

`Tools > Deucarian > Templates > Survivors > Runtime Debugger`

Click `Explicitly Reset Save / Progress`. Restart Play Mode to confirm the sample starts from a fresh local meta profile. The same window can start Standard or Sprint, grant blood shards for meta-upgrade checks, force a Sprint boss, inspect run metrics, and inspect the current build ranks, eligible evolutions, and current draft pool. Entering Play Mode, restarting the current run, and applying a pacing profile should not wipe saved meta progress.

## Enable Debug Fast

Debug Fast is opt-in only.

1. Enter Play Mode in the scene above.
2. Open `Tools > Deucarian > Templates > Survivors > Runtime Debugger`.
3. Set `Pacing Profile` to `DebugFast`.
4. Click `Apply Pacing Profile And Restart Current Run`.

Debug Fast uses faster spawns, faster escalation, quicker level-ups, shorter reward timeouts, and earlier boss timings for automated or quick validation. Do not use it for judging the default human-readable sample feel.
