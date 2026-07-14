# Validation

Phase 2L validation should cover:

- Unity compile in a fresh validation project.
- EditMode tests for descriptor creation, draft determinism, spawn, weapon damage/death, XP collection, upgrade selection, and magnet recall.
- PlayMode tests for first playable boot, run continuation after upgrade, player death, and restart.
- Manual sample scene open/play check from `Basic Survivors Game/Scenes/BasicSurvivorsGame.unity` under the current imported sample in `C:\Repositories\Template-Game-Survivors-Playtest`.
- Strict content validation and imported-scene smoke coverage for `Basic Survivors Game/Scenes/NeonArcana.unity` under that import.
- Asset-flip tests proving independent libraries, no Basic GUID borrowing, the same runtime/bootstrap, distinct names and palette, one starter, both modes, drafts, authored enemies/rewards/progression/tutorial/audio, and no alternate gameplay scripts.

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
- runtime and sample validation for default class IDs, exactly-one playable starting weapon/loadout references, and class-gated upgrade references
- clear validation failures for invalid class loadouts, missing/duplicate/empty class loadout entries, unknown default classes, unknown allowed classes, unknown gated upgrades, and duplicate class gates
- locked or missing selected class fallback to the default class
- default class starting with only the basic arcane wand loadout
- unlocked selected class starting with one authored weapon plus the expected stat profile
- class-specific upgrades appearing only when the selected class is valid
- existing first-slice, weapon, payload, run-flow, reward, relic, class unlock, save/load, and persistent-upgrade tests still passing

The Phase 2S class run-start and upgrade-gate systems remain local Survivors template-kit code. Passive skill trees, class capability tags, resource profiles, content packs, class starting upgrade graphs, and richer class-specific upgrade pools are deliberately deferred.

No package extraction, package publishing, Survivors template registration, Idle template mutation, or Movement-FPS migration is part of Phase 2S validation.

Phase 3E adds editor-side validation adoption:

- `Tools > Deucarian > Templates > Survivors > Validate Content` validates the package `Samples~/BasicSurvivorsGame` JSON libraries.
- The menu action runs the local `SurvivorsContentValidator`; Survivors-specific rules remain in this template.
- The editor report includes pickup JSON validation for required pickup ids, duplicate ids, display names, and behavior text. Pickup attraction values are validated in authored run flow.
- The editor runner converts local validation errors to Gameplay Foundation `ContentValidationReport` issues.
- Game Content Authoring formats and summarizes that report for console output only. Runtime assemblies do not reference editor-only authoring packages.
- EditMode coverage checks that the editor runner builds a report, valid sample content has no errors, invalid sample content appears through the shared authoring report path, and the runtime asmdef has no editor-only authoring references.

No gameplay behavior changes, shared package extraction, package publishing, Idle template mutation, or Movement-FPS migration is part of Phase 3E validation.

Phase 3F adds reference-parity gameplay validation:

- Default runtime content includes one authored starter weapon per fallback class, ten local weapon archetypes, one-rank unlock definitions for every authored weapon, nine enemy roles, six relics, and seven persistent upgrades.
- Run flow validation covers timed swarm, runner, bruiser, spitter, elite, miniboss, and boss pressure over a 30-minute sample arc.
- Sample JSON validation now enforces the vertical-slice minimums for weapon roles, complete weapon skill tracks, passive atlas count, enemy roles, elite variants, and boss/miniboss presence.
- EditMode coverage checks expanded enemy profiles plus barrier absorption and poison damage-over-time behavior.
- PlayMode coverage checks the one-weapon default loadout, draftable weapon unlocks, owned weapon unlock suppression, XP-gain and area-scaling passives, early/boss rarity weighting, weapon-owned mutation availability, and class-gated advanced weapon/passive availability.
- `Tools > Deucarian > Templates > Survivors > Runtime Debugger` provides Play Mode controls for forced XP, shard grants, level-ups, horde-rush trigger/clear checks, elite/miniboss/boss spawns, enemy bursts, arena fill, stress profiles, magnet recall, build/evolution/draft inspection, and meta reset.

Full graph-editor passive skill trees, production class content packs, production UI, monetization, networking, and shared package extraction remain deferred.

Phase 3G adds compact progression-atlas validation:

- runtime progression tracks for class passive atlases and weapon skill tracks
- EditMode coverage checks Blood Ring's Serrated Orbit branch and Thorn Halo's Bramble Guard rank path stay wired into their weapon tracks.
- sample `Content/DefaultProgression/progression.json` validation through the editor menu
- clear validation failures for duplicate progression track/node IDs, unknown class IDs, unknown weapon targets, unknown upgrade IDs, invalid node kinds, bad point costs, negative tiers, missing passive atlases, and node max ranks above the underlying upgrade rank
- class-specific upgrade gates derived from class-owned progression tracks instead of a separate hand-written list
- default Arcane passive upgrades and unlockable Ember passive/weapon upgrades appearing only for the selected class
- authored sample upgrade records defining explicit weights, max ranks, effect targets, numeric amounts, and rank/passive requirements so normal sample play does not borrow hidden fallback scalars

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
- Sprint progression validation targets first draft around `25`-`40` seconds, levels around `2`-`4` at 1 minute, `5`-`7` at 2 minutes, `8`-`10` at 3 minutes, `11`-`14` at 4 minutes, `14`-`18` at 5 minutes, and about `10`-`16` normal level-up drafts across a full 5-minute run.
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

- The sample scene binds all gameplay libraries, the pickup manifest, both themes, audio IDs, and tutorial copy through `BasicSurvivorsGameBootstrap` before run-mode selection. Enemies, run flow, and rewards were the first runtime-bound audit targets; Phase 3N makes the complete path strict.
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
- Sample scene smoke: covered by PlayMode tests dynamically resolving the current imported `Basic Survivors Game/Scenes/BasicSurvivorsGame.unity`.

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

Phase 3M adds Template Contract validation:

- `Documentation~/template-contract.md` defines the permanent rule: “Extract only reusable infrastructure, never the playable vertical slice.”
- README, structure, playtesting, package-boundary, sample, and extraction-candidate docs reference the contract and keep the Basic Survivors sample framed as a playable asset-flippable vertical slice.
- Package extraction candidates must document why they may be reusable, what is Survivors-specific, what stays local, second-template proof, likely package boundary, and vertical-slice preservation risk.
- EditMode coverage checks the contract document, required vertical-slice checklist items, protected local-content statements, package extraction candidates, and known hardening caveats remain documented.
- Existing authored-content, run-mode, UI, theme/audio/tutorial, Sprint/Standard, and sample scene tests remain the runtime guardrails for proving the template still plays after extraction.
- No package extraction, gameplay retuning, new weapons, new enemies, new upgrades, or shared package publishing is part of Phase 3M.

Phase 3N hardens authored content as the normal sample source of truth:

- `BasicSurvivorsGameBootstrap` uses `SurvivorsAuthoredContentBindingPolicy.StrictSample` and validates gameplay libraries, pickups, both themes, audio IDs, and tutorial copy before mode selection.
- Strict binding rejects missing gameplay-defining scalars and broken references instead of borrowing per-field values from `BasicSurvivorsGame`; failed binding keeps Standard/Sprint unavailable and reports the content error.
- `DefaultRunFlow/run-flow.json` contains a required `sharedGameplayTuning` baseline for 112 previously inherited live player, spawn, threat, pickup, status, barrier, waystone, and reward-feedback values. Optional non-zero `gameplayTuningOverrides` inherit from that authored baseline when omitted. The HUD's primary weapon damage/cooldown now resolves from the active authored weapon rather than duplicating weapon values in run flow.
- A parity test compares every shared/override field for Human Playtest, Sprint, Normal, Debug Fast, and Showcase against the previously effective built-in values, proving the hardening pass does not retune gameplay.
- Weapon/projectile, upgrade, enemy, Standard/Sprint run-flow, reward, live class-gate, and bound weapon/evolution-track metadata mutation tests prove JSON changes reach their intended runtime definitions. Missing required field tests prove strict validation fails rather than substituting fallback values.
- `DefaultProgression/progression.json` owns passive atlases, weapon tracks/nodes, evolution nodes, node costs/ranks, and class-specific gates. Class-atlas nodes drive live class eligibility; weapon-track point/tier metadata remains bound validation/tooling data, not a live point-spending system. `DefaultRewards/rewards.json` owns currencies, run/major/class-unlock grants, legacy XP, and persistent meta upgrades.
- Unbound and deliberately partial hosts remain supported through the explicit `AllowFallbacks` policy and report that fallback content is active.
- Game Content Authoring required no package change; the existing generic report adapter is sufficient and Survivors schema rules remain template-local.
- No package extraction, gameplay retuning, new content, or push is part of Phase 3N.

Latest local validation for Phase 3N on `2026-07-10`:

- Authored tuning capture: `112` shared fields across `5` profiles, `0` effective-value mismatches against the pre-hardening tuning factories.
- Package validator: passed with `Deucarian validation passed: com.deucarian.template.game.survivors`.
- Content validation: passed with `0` errors and `0` warnings; log at `C:\Repositories\_survivors_validation_tmp\authored-hardening-content-validation-final.log`.
- Restricted text scan: only the three allowed editor validation console calls in `Editor/SurvivorsEditorContentValidation.cs`; no TODO/FIXME/HACK markers.
- `git diff --check`: passed.
- EditMode: passed with `75` passed, `0` failed; final durable result at `C:\Repositories\_survivors_validation_tmp\authored-hardening-editmode-final-rerun.json`.
- PlayMode: the first full run passed `149` and exposed the known manual-simulation/frame-update Sprint race plus an obsolete generic-damage assertion. The test was made deterministic and the assertion was changed to the selected starter definition; the full rerun passed with `151` passed, `0` failed at `C:\Repositories\_survivors_validation_tmp\authored-hardening-playmode-rerun.json`.
- Imported sample smoke: covered by the full PlayMode rerun after refreshing the disposable validation project's imported sample from the current `Samples~/BasicSurvivorsGame` source. The source repository did not receive generated/imported files.
- Game Content Authoring package: inspected read-only; no generic package gap or package change was required.

Milestone 2B adds safe existing-record JSON edit validation:

- The existing Survivors content-pack provider implements the optional Game Content Authoring edit-provider contract for imported Basic Survivors and Neon Arcana packs.
- Only direct scalar fields on existing weapon, projectile, and enemy records are exposed. Stable IDs, references, roles, fire modes, arrays, structural fields, other categories, package sources, and All Packs remain read-only.
- A strict UTF-8 tokenizer and deterministic ID-based locator preserve BOM state, line endings, whitespace, ordering, unknown fields, unrelated numeric tokens, and every byte outside edited scalar spans. Full-file `JsonUtility` round trips are not used for gameplay JSON.
- Source revisions include the exact-byte hash, AssetDatabase GUID/path, pack key, ordered manifest source-list fingerprint, canonical record key, and backend schema token. Preview, commit, reimport verification, and rollback fail closed on stale state.
- Commit validates the complete proposed pack in memory, writes exact recovery bytes under `Library/Deucarian/GameContentAuthoring/Recovery/Survivors`, atomically replaces the existing file without delete-then-move, synchronously reimports, verifies bytes/tokens/revision, validates again, and reindexes. Resolved backups are bounded to five per source; unresolved recovery records are retained.
- Cancel changes zero bytes. Rollback restores exact pre-commit bytes only while the committed revision remains current, so later external edits are never overwritten.
- Editing is limited to project-owned imported copies under `Assets/Samples`. Package assets, PackageCache-backed content, arbitrary paths, traversal, reparse-point escapes, missing files, and read-only files are rejected. The first edit in an editor session warns that sample refresh may conflict with local edits.
- JSON remains the sole runtime source of truth. No ScriptableObject mirror, shipped content mutation, balance change, runtime gameplay change, record creation, deletion, duplication, or pack cloning is part of this milestone.

Latest local validation for Milestone 2B on `2026-07-13`:

- Required Game Content Authoring baseline: clean `develop` at `c6b45bee6d6bba7fb6a96b5dcc278d0f01750e01`.
- Package validator: passed with `Deucarian validation passed: com.deucarian.template.game.survivors`.
- `git diff --check`: passed.
- Survivors EditMode: final full rerun passed `134` passed, `0` failed. The focused tokenizer/locator/patch set passed `21`; the focused provider/session/transaction set passed `15`.
- Game Content Authoring compatibility: all `83` GCA EditMode tests passed in the disposable integration project; the combined GCA plus Survivors run passed `217` passed, `0` failed.
- Survivors PlayMode: full run passed `154` passed, `0` failed. Focused Basic Standard, Basic Sprint, Neon Standard, and Neon Sprint smokes each passed `1` passed, `0` failed.
- Runtime mutation proof: Basic and Neon each committed an enemy-health scalar through the provider/session API, launched the matching strict-authored scene, observed the committed authored profile at runtime without fallback, and restored the exact original bytes and hash.
- Source preservation: all `22` tracked sample content JSON files matched their `HEAD` blob hashes after validation; the imported Basic and Neon enemy JSON copies matched the package SHA-256 values after runtime proof cleanup.
- One initial full EditMode attempt encountered transient Windows file contention during a Neon atomic-replacement test (`133` passed, `1` failed). The destination remained recoverable and exact, the focused transaction rerun passed `15/15`, and the clean full rerun passed `134/134`.

Milestone 2D1 extends the same transaction backend with one canonical reference field:

- Evolution records expose only required `requiredPassiveUpgradeId` plus read-only `id`; no other upgrade or reference field is writable.
- The shared selector supplies canonical same-pack candidates with Upgrade and Passive capabilities. Survivors authoritatively rejects cross-pack, crafted, missing, invalid, non-Passive, and stale targets before converting the approved key to its existing JSON ID.
- Preview and Commit run complete selected-pack validation. Runtime validation independently requires every authored passive prerequisite to resolve to a Passive record.
- Mutation coverage compares the committed file to an exact one-token lossless patch for Basic and Neon, verifies strict runtime evolution metadata consumes the new prerequisite without fallback, and restores the original bytes.

Latest local validation for Milestone 2D1 on `2026-07-13`:

- Focused editing/reference/transaction suite: `21` passed, `0` failed.
- Complete Survivors EditMode suite: `140` passed, `0` failed, including strict Basic and Neon content validation.
- Complete Survivors PlayMode suite: `154` passed, `0` failed.
- Dedicated imported-scene smokes: Basic Standard, Basic Sprint, Neon Standard, and Neon Sprint each passed `1`, failed `0`.
- Shared package validator and `git diff --check`: passed.
- Package metadata, dependency versions, asmdefs, authored sample content, content-pack manifests, and scenes: no diff.

Milestone 2D2C adds one safe ordered scalar collection field:

- Existing tutorial-step records in the Basic and Neon UI-theme sources expose only `Lines` plus the read-only step ID. Other arrays, tutorial-step creation/deletion, IDs, and structure remain read-only.
- `Lines` uses the shared `OrderedScalarCollection<string>` contract with stable item keys, Add, Remove, Move Up, Move Down, Replace, Restore Original Order, Undo, Redo, and Cancel support. Duplicate text is allowed and item identity does not depend on the text value.
- The authoritative panel capacity is `1` to `3` nonblank lines. The upper bound matches the existing fixed three-row small-screen tutorial panel; this milestone does not redesign or scroll the runtime UI.
- The template-owned tokenizer replaces only the unique direct `lines` array span. It preserves UTF-8 BOM state, line endings, indentation, compact or multiline layout, property ordering, unknown fields, unrelated arrays, and every byte outside that span.
- Preview and Commit validate the complete selected pack. Collection operations, Undo, Redo, preview, commit, and rollback retain the existing stale-revision, atomic-write, backup, recovery, reimport, and strict-runtime verification rules.
- Strict Basic and Neon runtime binding consumes committed line text and order directly from authored JSON without fallback or a ScriptableObject mirror. Sample reimport can overwrite edits to the project-owned imported copy, so authors should review or commit those changes first.

Latest local validation for Milestone 2D2C on `2026-07-14`:

- Focused lossless tokenizer/locator/patch suite: `33` passed, `0` failed.
- Focused provider/session/transaction suite: `33` passed, `0` failed, including repair of both blank and zero-line invalid tutorial copy; dedicated Basic and Neon commit/runtime/rollback rerun: `2` passed, `0` failed.
- Tutorial panel-capacity validation: `1` passed, `0` failed.
- Complete Survivors EditMode suite with one job worker: `165` passed, `0` failed. Earlier broad attempts intermittently reported fail-closed Windows `File.Replace` contention in unchanged transaction tests; the exact tests reran cleanly and the final low-worker full run passed.
- Complete Game Content Authoring EditMode suite: `103` passed, `0` failed (`93` core/pack tests and `10` generic ordered-collection editor tests).
- Complete Survivors PlayMode suite: `154` passed, `0` failed.
- Strict independent Basic and Neon content-set validation: `1` passed, `0` failed.
- Dedicated imported-scene smokes: Basic Standard, Basic Sprint, Neon Standard, and Neon Sprint each passed `1`, failed `0`.
- Package validator and `git diff --check`: passed.
- All `22` shipped content JSON files match their `HEAD` Git blob hashes; all `22` disposable imported copies match package-source SHA-256 hashes after tests. The package's `106` Unity metadata GUIDs have no duplicates.
- Package metadata, dependency versions, asmdefs, authored JSON, content-pack manifests, scenes, gameplay tuning, weapons, enemies, upgrades, and rewards: no diff.

Transactional file-replacement hardening adds bounded handling for known transient Windows replacement contention without changing authored content or the transaction's fail-closed state model:

- Atomic replacement records the operation stage, safe destination, exception type, full `HResult`, available Windows code, attempt number, and retry disposition. A direct locked-handle probe on the validation machine produced `System.IO.IOException`, `HResult 0x80070020`, and low-word code `32`, proving the managed exception path retains the classification needed here; it does not identify the process responsible for earlier intermittent contention.
- Only replacement-stage `IOException` codes `32`, `33`, and `1175` retry. Access denial, `UnauthorizedAccessException`, codes `1176`/`1177`, unknown codes, and every non-replacement failure stop immediately.
- The initial attempt plus at most three retries use `25`, `75`, and `200` millisecond delays. Before retry, exact destination and immutable prepared-file hashes plus the complete session source target/revision are rechecked. Import remains after successful replacement only.
- Commit, explicit rollback, and automatic recovery restoration share the same rule. Existing original/proposed/unknown-hash classification remains authoritative after failure.
- The atomic support probe caches success and permanent failure. Exhausted transient failures use a one-second cooldown and can be probed again instead of making editing permanently unavailable for the editor session.

Latest local validation for transactional file-replacement hardening on `2026-07-14`:

- Focused atomic classification/retry/probe tests: `17` passed, `0` failed.
- Focused Survivors provider/session/transaction tests: `37` passed, `0` failed.
- Complete Survivors EditMode suite with one job worker: `186` passed, `0` failed.
- Complete Game Content Authoring compatibility: `93` core tests and `10` generic editor tests passed; the combined GCA plus Survivors inventory passed `289`, failed `0`.
- Complete Survivors PlayMode suite: `154` passed, `0` failed.
- Basic and Neon authored content validation: `0` errors, `0` warnings.
- Package validator: passed with `Deucarian validation passed: com.deucarian.template.game.survivors`.
- `git diff --check`: passed.
- All `22` shipped sample JSON files remained outside the source diff. Transaction tests restored every edited imported file to its exact pretest bytes; a final line-ending-normalized comparison matched all `22` imported files to the shipped source content. The disposable Windows host keeps CRLF working copies, so package-to-import raw hashes are not claimed.
- Validation artifacts: `C:\Repositories\_survivors_validation_tmp\transaction-hardening\atomic-focused-final.json`, `editing-focused.json`, `editmode-full.json`, `editmode-combined.json`, `gca-editmode.json`, `gca-editor-editmode.json`, `playmode-full.json`, and `content-validation.log`.

The first production structured-row backend exposes only existing Upgrade `Effects` arrays in imported Basic Survivors and Neon Arcana packs:

- Effect rows remain anonymous values owned by the parent Upgrade. They have session-only keys and original indexes, but no authored ID, canonical record, inbound reference, save identity, source record, or top-level CRUD lifecycle.
- The row schema exposes closed `effect` and `target` enums plus a finite non-zero numeric `amount`. Add requires all three explicit values; no hidden gameplay default is generated. Remove, Move, field replacement, Restore Original Order, Undo, and Redo remain staged until Commit.
- The locator resolves one parent Upgrade by stable ID, one unique direct `effects` array, direct row objects, and exact mapped child tokens. Unsupported nested/ambiguous rows fail closed; unknown direct scalar properties retain their parsed spans and exact raw bytes without becoming writable.
- Unchanged and moved rows reuse exact raw object text. Existing-row edits replace only changed direct child tokens. Added rows use deterministic field order and source-local compact/multiline formatting. The transaction replaces only the direct array span and retains every byte outside it.
- Preview and Commit reuse complete strict selected-pack validation, stale/source locks, the hardened `File.Replace` retry/recovery path, synchronous reimport/reindex, committed-value verification, and revision-checked exact rollback. JSON remains the sole source of truth.

Latest local validation for Upgrade Effects structured-row editing on `2026-07-14`:

- Required baselines: Game Content Authoring `df2908fb870cdae193bc5b1df4d276180419c86e`; Survivors `5c9077286d51ffc2b972a933a79c49005df42c63`; both clean and synchronized before implementation.
- Focused Effects provider/session/transaction/runtime suite: `12` passed, `0` failed. Focused lossless locator/row-span/format suite: `23` passed, `0` failed. Existing editing/reference/transaction compatibility fixture: `37` passed, `0` failed.
- Strict independent Basic and Neon content-set validation: `1` passed, `0` failed. Complete Survivors EditMode: `221` passed, `0` failed. Complete Survivors PlayMode: `154` passed, `0` failed.
- Dedicated imported-scene smokes: Basic Standard, Basic Sprint, Neon Standard, and Neon Sprint each passed `1`, failed `0`.
- Complete unchanged Game Content Authoring EditMode dependency suite: `119` passed, `0` failed.
- Runtime mutation proof committed an amount edit plus row move for `Keen Edge` independently in Basic and Neon, verified catalog order/values and controller application under strict binding with fallback false, then restored exact original bytes. Each proof also staged and removed a complete added row; the other pack hash remained unchanged.
- Package validator: passed with `Deucarian validation passed: com.deucarian.template.game.survivors`. `git diff --check`: passed. Metadata scan: `110` GUIDs with no duplicates.
- All `22` shipped sample JSON files match their `HEAD` Git blobs after clean filters. Imported Basic and Neon upgrade copies exactly match package SHA-256 values `6A5FA6B64CD19AA1ECDABAB2A22CA14302418F6CA51656C5D9A99EA57DF1B6BA` and `6C92F11A1CAA0F395E79A0D61A0BD43F47BA2B1FA0017E5CF8F780EC2D32C034` after testing.
- Runtime files, gameplay tuning, authored values, sample JSON, scenes, package/dependency versions, and asmdefs have no diff. Game Content Authoring and every other repository remained unchanged.
