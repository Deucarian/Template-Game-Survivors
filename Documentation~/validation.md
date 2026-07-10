# Validation

Phase 2L validation should cover:

- Unity compile in a fresh validation project.
- EditMode tests for descriptor creation, draft determinism, spawn, weapon damage/death, XP collection, upgrade selection, and magnet recall.
- PlayMode tests for first playable boot, run continuation after upgrade, player death, and restart.
- Manual sample scene open/play check from `Assets/Samples/com.deucarian.template.game.survivors/Basic Survivors Game/Scenes/PLAYTEST_THIS_SCENE_Survivors_Game.unity` in `C:\Repositories\Template-Game-Survivors-Playtest`.

The reference Vampire clone has local working-tree edits in UI files during this phase and is treated as read-only input.

Phase 2M adds validation for:

- local weapon archetype descriptor coverage
- orbit upgrade application
- orbit weapon damage/death
- melee weapon damage/death
- burst weapon damage/death
- run upgrade affecting a new weapon archetype

Phase 2N adds validation for:

- runtime catalog content validation
- sample JSON load and validation for weapons/projectiles/upgrades
- clear validation failures for duplicate weapon IDs, duplicate upgrade IDs, invalid archetypes, missing projectile references, and invalid upgrade targets
- hitscan/beam damage and enemy death
- projectile pierce behavior
- projectile chain retarget behavior
- projectile fork/split spawn behavior
- projectile return/boomerang behavior
- run upgrade affecting a new projectile modifier

No package extraction, package publishing, Survivors template registration, Idle template mutation, or Movement-FPS migration is part of Phase 2N validation.

Phase 2O adds validation for:

- payload runtime descriptor coverage
- payload sample JSON timing and radius validation
- clear validation failures for invalid payload count, travel speed, arming time, lifetime, trigger radius, explosion radius, hazard duration, hazard tick interval, and negative hazard damage ratio
- grenade payload detonation, area damage, and enemy death
- placed trap arming/proximity trigger, area damage, and enemy death
- run upgrade affecting payload behavior

No package extraction, package publishing, Survivors template registration, Idle template mutation, or Movement-FPS migration is part of Phase 2O validation.

Phase 2P adds validation for:

- local run-flow descriptor and escalation validation
- sample enemy JSON validation for swarm, miniboss, and boss definitions
- clear validation failures for invalid boss/miniboss role, timing, health, move speed, radius, contact interval, contact damage, XP, and duplicate enemy IDs
- timed miniboss and boss spawn smoke
- miniboss death and XP drop smoke
- ranged enemy attack feedback and player-damage smoke
- major reward drop feedback, cache-pull pickup bursts, and slam ground telegraphs for elite, miniboss, dread elite, and boss kills
- boss death and victory trigger smoke
- survival-duration victory smoke
- existing defeat/restart flow

No package extraction, package publishing, Survivors template registration, Idle template mutation, or Movement-FPS migration is part of Phase 2P validation.

Phase 2Q adds validation for:

- runtime reward/meta descriptor validation
- sample reward JSON validation for blood shards, legacy XP, persistent upgrades, and reward definitions
- clear validation failures for duplicate currency IDs, duplicate persistent upgrade IDs, invalid persistent upgrade targets, missing persistent upgrade effects, invalid rank costs, duplicate reward IDs, missing reward currency/track references, and empty rewards
- reference-shaped run reward calculation for duration, level, miniboss kills, boss kills, victory, and boss bonus rewards
- v1-to-v2 meta profile save migration
- miniboss reward grant when a run ends in defeat
- final boss reward grant and victory reward persistence
- save/load persistence across controller instances
- persistent meta upgrades affecting a later run's projectile damage, max health, pickup range, XP gain, and reroll charges

The Phase 2Q save path uses `com.deucarian.persistence` and `com.deucarian.progression` from local Survivors kit code. Boss relic drafts, class unlocks, skill trees, and concrete-product reward economies are handled by later local slices rather than shared package extraction.

No package extraction, package publishing, Survivors template registration, Idle template mutation, or Movement-FPS migration is part of Phase 2Q validation.

Phase 2R adds validation for:

- runtime relic/class descriptor validation
- sample relic JSON validation for unique IDs, valid targets, valid effect kinds, positive weights, and valid effect amounts
- sample class JSON validation for unique IDs, valid starting weapons, default unlock availability, unlock reward IDs, and valid stat modifiers
- v2-to-v3 meta profile shape through selected/unlocked class fields
- class unlock and selected class persistence through the local meta profile
- miniboss death opening a boss relic draft
- selected relic affecting the current run
- final boss victory granting the sample class unlock
- selected class affecting a new run's starting move speed, damage, and max health
- persisted class unlock selection across controller instances

The Phase 2R relic and class systems remain local Survivors template-kit code. Skill trees, class-specific upgrade pools, class resource profiles, content-pack gates, boss relic rarity tiers, and reward-selection timeout behavior are deliberately deferred.

No package extraction, package publishing, Survivors template registration, Idle template mutation, or Movement-FPS migration is part of Phase 2R validation.

Phase 2S adds validation for:

- runtime class loadout descriptor coverage
- runtime and sample validation for default class IDs, starting weapon/loadout references, and class-gated upgrade references
- clear validation failures for invalid class loadouts, missing/duplicate/empty class loadout entries, unknown default classes, unknown allowed classes, unknown gated upgrades, and duplicate class gates
- locked or missing selected class fallback to the default class
- default class starting with only the basic arcane wand loadout
- unlocked selected class starting with the expected advanced loadout and stat profile
- class-specific upgrades appearing only when the selected class is valid
- existing first-slice, weapon, payload, run-flow, reward, relic, class unlock, save/load, and persistent-upgrade tests still passing

The Phase 2S class run-start and upgrade-gate systems remain local Survivors template-kit code. Passive skill trees, class capability tags, resource profiles, content packs, class starting upgrade graphs, and richer class-specific upgrade pools are deliberately deferred.

No package extraction, package publishing, Survivors template registration, Idle template mutation, or Movement-FPS migration is part of Phase 2S validation.

Phase 3E adds editor-side validation adoption:

- `Tools > Deucarian > Templates > Survivors > Validate Content` validates the package `Samples~/BasicSurvivorsGame` JSON libraries.
- The menu action runs the local `SurvivorsContentValidator`; Survivors-specific rules remain in this template.
- The editor report includes pickup JSON validation for required pickup ids, duplicate ids, display names, attraction values, and behavior text.
- The editor runner converts local validation errors to Gameplay Foundation `ContentValidationReport` issues.
- Game Content Authoring formats and summarizes that report for console output only. Runtime assemblies do not reference editor-only authoring packages.
- EditMode coverage checks that the editor runner builds a report, valid sample content has no errors, invalid sample content appears through the shared authoring report path, and the runtime asmdef has no editor-only authoring references.

No gameplay behavior changes, shared package extraction, package publishing, Idle template mutation, or Movement-FPS migration is part of Phase 3E validation.

Phase 3F adds reference-parity gameplay validation:

- Default runtime content includes five starting weapons, ten local weapon archetypes, one-rank unlock definitions for every authored weapon, nine enemy roles, six relics, and seven persistent upgrades.
- Run flow validation covers timed swarm, runner, bruiser, spitter, elite, miniboss, and boss pressure over a 30-minute sample arc.
- Sample JSON validation now enforces the vertical-slice minimums for weapon roles, complete weapon skill tracks, passive atlas count, enemy roles, elite variants, and boss/miniboss presence.
- EditMode coverage checks expanded enemy profiles plus barrier absorption and poison damage-over-time behavior.
- PlayMode coverage checks the expanded default loadout, draftable weapon unlocks, owned weapon unlock suppression, XP-gain and area-scaling passives, early/boss rarity weighting, weapon-owned mutation availability, and class-gated advanced weapon/passive availability.
- `Tools > Deucarian > Templates > Survivors > Runtime Debugger` provides Play Mode controls for forced XP, shard grants, level-ups, horde-rush trigger/clear checks, elite/miniboss/boss spawns, enemy bursts, arena fill, stress profiles, magnet recall, build/evolution/draft inspection, and meta reset.

Full graph-editor passive skill trees, production class content packs, production UI, monetization, networking, and shared package extraction remain deferred.

Phase 3G adds compact progression-atlas validation:

- runtime progression tracks for class passive atlases and weapon skill tracks
- EditMode coverage checks Blood Ring's Serrated Orbit branch and Thorn Halo's Bramble Guard rank path stay wired into their weapon tracks.
- sample `Content/DefaultProgression/progression.json` validation through the editor menu
- clear validation failures for duplicate progression track/node IDs, unknown class IDs, unknown weapon targets, unknown upgrade IDs, invalid node kinds, bad point costs, negative tiers, missing passive atlases, and node max ranks above the underlying upgrade rank
- class-specific upgrade gates derived from class-owned progression tracks instead of a separate hand-written list
- default Arcane passive upgrades and unlockable Ember passive/weapon upgrades appearing only for the selected class

The Phase 3G atlas is intentionally compact local template-kit data. It is not the reference clone's full passive graph UI, per-weapon graph editor, class resource economy, or twelve-class content-pack ecosystem.

Phase 3H adds reward-selection timeout validation:

- non-default validation profiles can include a reward-selection countdown for level-up and boss relic drafts
- `Simulate` continues ticking the paused reward state so automated tests and gameplay both resolve choices
- PlayMode coverage checks level-up auto-pick and boss relic auto-pick after a short timeout
- manual choice selection still clears the timer and resumes the run

The timeout remains local template UI/runtime behavior. No production UI framework or shared reward-surface package is introduced.

Phase 3I adds human-readable pacing validation:

- `SurvivorsPacingProfile.HumanPlaytest` is the default profile and keeps `Time.timeScale` at `1`.
- Human Playtest opening spawns use two-enemy packs on a `0.95` second interval, `38` maximum alive enemies, `1.35` basic enemy speed against `5.55` player speed, readable `9.25` projectile speed, `2.9` pickup magnet range, and no reward timeout.
- Human Playtest max alive rises by `8` about every `45` seconds, spawn pack size grows with escalation up to `6`, the spawn interval drops by `0.08` seconds per escalation, major threats warn before they enter, the first horde rush begins around `75` seconds, the first elite arrives around `135` seconds, the dread elite arrives around `255` seconds, the miniboss waits until `360` seconds, the final boss appears around `1140` seconds, and survival victory lands at `1800` seconds.
- `SurvivorsPacingProfile.SprintRun` is a separate selectable 5-minute profile with its own target duration, victory time, boss timing, elite timing, horde timing, XP curve, level-up draft cooldown, queued draft cap, rarity ramp, reward multiplier, and evolution assist. It does not retune Human Playtest.
- Sprint progression validation targets first draft around `25`-`40` seconds, levels around `3`-`4` at 1 minute, `5`-`7` at 2 minutes, `8`-`10` at 3 minutes, `11`-`14` at 4 minutes, `14`-`18` at 5 minutes, and about `10`-`16` normal level-up drafts across a full 5-minute run.
- Sprint standing-still validation checks that the opening becomes lethal or seriously dangerous within `60`-`120` seconds without a strong build.
- Pickup-build validation covers authored Gemheart, Lodestone Sigil, and Vacuum Pulse choices, pickup range, gem pull speed, periodic recall behavior, and queue/throttle protection against draft floods.
- Enemy lifecycle validation covers authored recycle/leash/reposition/marker/life-bar flags, normal enemy offscreen recycling, persistent major-threat health preservation, offscreen major-threat markers, boss health bar integration, and removal of the old player-centered circle/four-bar placeholder visuals from normal play.
- `SurvivorsPacingProfile.Normal`, `SurvivorsPacingProfile.DebugFast`, and `SurvivorsPacingProfile.Showcase` are explicit profiles for validation and demo work, not sample defaults.
- EditMode coverage checks Human Playtest readability thresholds, Sprint timing/reward/evolution knobs, sample run-flow content for all required profiles, pickup-build upgrade coverage, enemy lifecycle authoring, and Debug Fast acceleration as opt-in tuning.
- PlayMode coverage checks waystone discovery, Waystone Chain, roaming cache special-drop, ambush, ambush-clear, and Arena Trial payoff beats using deterministic discovery/cache/travel cadence tuning.
- PlayMode coverage checks the run-mode selector can start Standard and Sprint, Standard still starts Human Playtest, the imported scene starts by path and advances into horde spawning plus a three-choice XP draft, the first XP pickup can be collected within seconds, the first Standard draft can open within one minute, Sprint can open the first draft in the target window, Sprint remains inside the 5-minute level/draft envelope, Sprint standing still causes meaningful danger, Sprint can reach evolution eligibility with its separate assist, Sprint summary includes the selected mode, major-threat warnings appear before timed elites/miniboss/boss, reward choices wait for the player by default, pickup/magnet builds affect collection without bypassing draft pacing, offscreen major-threat markers and health state persist through repositioning, normal enemies recycle outside the hard radius, placeholder player-circle/bar visuals are absent in normal play, and Debug Fast only appears through an explicit profile switch.
- Combat, pickup, and reward feedback coverage checks enemy hit flashes, short-lived enemy death bursts, magnet recall feedback, evolution-triggered XP recall, normal level-up Level Pulse damage, full-weapon Arsenal Surge momentum, full-passive Harmony Surge momentum, Waystone Chain momentum, Arena Trial Shrine Surge momentum, boss relic Relic Surge momentum, endless major-threat Endless Surge rewards, low-health Clutch Pulse safety and enemy damage, multi-evolution Legend Surge momentum, Gem Rush pickup-cluster stat boosts, XP attraction feedback, pulsing recalled gem presentation, rarity-card presentation, and selected reward banners.
- PlayMode persistence coverage uses isolated in-memory save slots to prove normal start, Normal profile application, and run restart do not wipe meta progression, while explicit reset does wipe it.
- `Documentation~/playtesting.md` documents the local playtest host project, renamed imported sample scene, Standard and Sprint timing expectations, explicit save reset, and Debug Fast workflow.

The pacing profiles remain local template tuning. No package extraction or shared pacing framework is introduced.

Phase 3J adds audit-gap validation for authored runtime binding and HUD readability:

- The sample scene binds `Samples~/BasicSurvivorsGame/Content/DefaultEnemies/enemies.json`, `Samples~/BasicSurvivorsGame/Content/DefaultRunFlow/run-flow.json`, and `Samples~/BasicSurvivorsGame/Content/DefaultRewards/rewards.json` through `BasicSurvivorsGameBootstrap` before run-mode selection.
- EditMode coverage checks that authored Human Playtest/Sprint run-flow profiles, authored elite/miniboss/boss IDs, major reward IDs, rarity tables, and lifecycle/life-bar flags are used by runtime definitions, and that invalid authored reward references fail validation.
- PlayMode coverage checks the dedicated top-center timer, exact Sprint `01:42` level/draft bounds, and concurrent major-threat life-bar state.
- Manual validation should confirm the top-center timer remains readable with warnings, authored overhead elite/miniboss bars appear onscreen, authored boss bars appear for bosses, offscreen markers still preserve major-threat health, and authored reward caches remain recoverable.

Phase 3K adds player-facing UI polish validation:

- Runtime coverage checks the run-mode selector still appears at boot and Standard/Sprint still start their existing profiles.
- EditMode coverage checks `Content/DefaultUiTheme/ui-theme.json` loads and exposes mode labels, rarity labels, category labels, and draft button copy for asset flips.
- PlayMode coverage checks normal gameplay starts with the debug overlay hidden, F1/debug toggle state resets on mode change, full-screen draft cards expose name/rarity/category/affected/rank/description/effect/hotkey data, numbered draft hotkeys still select choices, and the build menu shows owned weapons, passives, relics, pickup/magnet stats, run info, and controls.
- Manual validation should confirm the normal HUD looks player-facing, the old debug wall appears only when toggled, and `Tab`, `B`, or `Esc` opens and closes the build menu without conflicting with draft `1`/`2`/`3` selection.

Required local validation for this pass:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
git diff --check
```

Run Unity EditMode and PlayMode tests after those checks. Run the editor content validation menu or its equivalent test coverage after editing authored JSON. Record any machine-local XML/log artifact paths in the task handoff rather than committing bulky generated logs.

Latest local validation for Phase 3J on `2026-07-10`:

- Package validator: passed with `Deucarian validation passed: com.deucarian.template.game.survivors`.
- Content validation: passed through the EditMode sample JSON and authored runtime binding tests.
- Restricted text scan: no new restricted markers in this pass; the scan still reports the existing editor content-validation console bridge in `Editor/SurvivorsEditorContentValidation.cs`.
- `git diff --check`: passed.
- EditMode: passed with `49` passed, `0` failed, durable runner callback completed.
- PlayMode: initial full run reported one transient Sprint first-draft `ArgumentOutOfRangeException`; the focused test rerun passed, then the full rerun passed with `138` passed, `0` failed, durable runner callback completed.
- Sample scene smoke: covered by PlayMode tests loading `Assets/Samples/com.deucarian.template.game.survivors/Basic Survivors Game/Scenes/PLAYTEST_THIS_SCENE_Survivors_Game.unity`.

Commands used:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
git diff --check
Unity.exe -batchmode -nographics -projectPath <playtest-host-project> -executeMethod Deucarian.TestAutomation.BatchTestRunner.RunEditMode -batchTestResults <temp-results-json> -batchTestTimeoutSeconds 900 -logFile <temp-log>
Unity.exe -batchmode -nographics -projectPath <playtest-host-project> -executeMethod Deucarian.TestAutomation.BatchTestRunner.RunPlayMode -batchTestResults <temp-results-json> -batchTestTimeoutSeconds 1800 -logFile <temp-log>
```

Phase 3L adds product-polish validation:

- Run Summary 2.0 coverage checks victory and defeat summaries expose mode, result, time, level, kills, elite/miniboss/boss kills, XP, rewards, meta bank, reward multiplier, damage taken, final health, weapons, passives, evolutions, relics, pickup/magnet stats, best moment, Restart Same, Change Mode, and Standard endless Continue when available.
- Draft-card coverage checks comparison previews for numeric stat changes, weapon unlocks, pickup radius, magnet pulse interval, and evolution rewards without exposing internal IDs.
- Tutorial coverage checks first-time onboarding appears after mode selection, pauses simulation, supports next/back/skip, persists the tutorial-seen flag, and does not reappear after the profile is marked seen.
- Audio coverage checks themeable event IDs, mute behavior, repetitive-event throttling, mode selection, draft opening, draft choice selection, and missing-clip-safe dispatch.
- Theme coverage checks the default UI theme and `Neon Arcana` alternate theme load, expose rarity/category/result/tutorial/audio tokens, and can be selected from the mode screen.
- Mobile/small-screen coverage checks the shared centered panel primitive stays inside a small game view for the mode selector, draft overlay, tutorial, build menu, and run summary. Manual QA should still confirm the actual rendered selector, draft cards, tutorial, build menu, and result screen remain readable at small resolutions.
- Documentation coverage includes Run Summary 2.0, comparison previews, tutorial flow, audio event palette, small-screen testing, Neon Arcana theme selection/editing, debug-only UI, and package extraction candidates.
- No gameplay retuning, new weapons, new enemies, new upgrades, package extraction, or shared package publishing is part of Phase 3L.
