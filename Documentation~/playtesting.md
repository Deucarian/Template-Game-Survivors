# Survivors Playtesting

Use the clear local host project for human playtesting:

`C:\Repositories\Template-Game-Survivors-Playtest`

The host references this package directly:

`com.deucarian.template.game.survivors: file:C:/Repositories/Template-Game-Survivors`

Open this imported/editable scene:

`Assets/Samples/com.deucarian.template.game.survivors/Basic Survivors Game/Scenes/BasicSurvivorsGame.unity`

Press Play from that scene. The sample scene should start immediately with no setup menu or repair step.

## Default Profile

The sample defaults to `SurvivorsPacingProfile.Normal`.

Normal uses `Time.timeScale == 1`, an enemy spawn interval of `1.35` seconds, an opening maximum of `36` alive enemies, basic enemy speed of `1.45` versus player speed of `5.4`, projectile speed of `8.25`, pickup attract range of `2.35`, and a `45` second reward-choice timeout.

## What To Check

First 30 seconds:

- The scene starts in Normal, not Debug Fast.
- Basic swarm enemies enter slowly enough to read movement, dodge spacing, pickup gems, and the starting weapon kit.
- Runners should not appear until about 35 seconds.
- Level-up choices should not spam instantly.
- No console errors, missing scripts, missing materials, or missing package references should appear.

After 2 minutes:

- The run should have opened at least one readable level-up choice.
- Runners and bruisers should be present, but the screen should still be understandable.
- Rewards should stay open long enough to read or choose with mouse, `1`, `2`, or `3`.
- Restart after defeat should still work with `R` or the restart button.

After 5 minutes:

- Spitters should have joined the enemy mix.
- The miniboss should arrive as a noticeable event and defeating it should open a boss relic choice.
- The game should feel busier than the first minute without becoming immediate visual noise.

## Reset Save Data

Enter Play Mode, then open:

`Tools > Deucarian > Templates > Survivors > Runtime Debugger`

Click `Reset Save / Meta Progression`. Restart Play Mode to confirm the sample starts from a fresh local meta profile.

## Enable Debug Fast

Debug Fast is opt-in only.

1. Enter Play Mode in the scene above.
2. Open `Tools > Deucarian > Templates > Survivors > Runtime Debugger`.
3. Set `Pacing Profile` to `DebugFast`.
4. Click `Apply Pacing Profile And Restart`.

Debug Fast uses faster spawns, faster escalation, quicker level-ups, shorter reward timeouts, and earlier boss timings for automated or quick validation. Do not use it for judging the default human-readable sample feel.
