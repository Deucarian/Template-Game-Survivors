# Survivors Template Structure

## Template Contract

`Documentation~/template-contract.md` is the extraction and refactor contract for this repository. It keeps the Basic Survivors sample as a complete playable vertical slice: open the imported sample scene, press Play, choose a mode, and play. Package extraction may move reusable infrastructure only when the sample scene, authored content, tuning, UI, themes, audio palette, tutorial copy, and playable loop remain wired and validated.

## Runtime Kit

`Runtime/BasicSurvivorsGame.cs` contains sample-kit tuning shape, stable IDs, descriptors, and safe fallback defaults used only by tests, debug tooling, legacy partial binding, and unbound hosts. Strict Basic sample binding does not use these catalogs to fill missing authored values:

- `SurvivorsTemplateTuning`
- `SurvivorsPacingProfile`
- `BasicSurvivorsGame`
- `SurvivorsRunState`
- `SurvivorsPickupKind`

`Runtime/SurvivorsTemplateController.cs` contains the run composition root, local actors, and local genre adapters:

- `SurvivorsTemplateController`
- `SurvivorsEnemyActor`
- `SurvivorsProjectileActor`
- `SurvivorsPickupActor`
- `SurvivorsSpawnPoseResolver`
- Human Playtest, Sprint Run, Normal, Debug Fast, and Showcase pacing profile selection
- strict authored weapon, upgrade, relic, class, progression, enemy, run-flow, reward, pickup, UI theme, audio palette, and tutorial-copy binding for the playable sample, with fallback defaults reserved for identifiable unbound/debug hosts
- pre-run failure state that disables mode start and displays the validation error when strict sample content is incomplete
- dedicated top-center elapsed/remaining timer HUD
- clean player HUD, F1-gated debug overlay, pre-run mode/theme selector, first-time tutorial overlay, full-screen draft-card overlay, and pause/build menu tabs for Current Build, Stats, Run Info, and Controls
- timed reward draft behavior for level-up choices, miniboss reward chains, and boss relic choices
- current-to-next rank labels plus player-facing comparison previews for live draft cards and debug draft inspection
- Run Summary 2.0 lines for mode, result, time, level, kills, XP, rewards, damage taken, final health, weapons, passives, evolutions, relics, pickup/magnet stats, best moment, result buttons, and meta/class options
- themeable audio event routing for UI, draft, pickup, combat, warning, reward, victory/defeat, and run-summary events
- empty level-up draft fallback that grants the skip reward and resumes play
- full-weapon Arsenal Surge feedback, nearby non-major enemy damage, and short stat momentum
- full-passive Harmony Surge feedback, nearby non-major enemy damage, and short stat/XP momentum
- boss relic Relic Surge feedback, nearby non-major enemy damage, and short stat momentum
- post-victory Endless Surge feedback, tiered reward drops, nearby non-major enemy damage, and short stat momentum for recurring major-threat kills
- travel-triggered Arena Trials with tracked enemy rings, XP/shard clear rewards, nearby non-major enemy damage, and short stat momentum
- normal level-up Level Pulse feedback and nearby non-major enemy damage
- one-shot low-health Clutch Pulse feedback, nearby non-major enemy damage, and brief player safety
- victory/defeat summary lines shared by tests and the IMGUI result panels
- weapon-evolution surge feedback, nearby non-major enemy damage, and loose-XP recall
- multi-evolution Legend Surge feedback, nearby non-major enemy damage, and short stat momentum
- timed horde-rush events that warn the player, spawn mixed enemy rings between major threats, and drop clear-reward caches
- local enemy crowd-spacing force for readable horde presentation
- concurrent major-threat health readouts, including authored overhead bars for elites/minibosses and prominent boss bars
- offscreen edge markers and safe re-entry behavior for distant persistent elites, dread elites, minibosses, and bosses
- normal enemy leash/recycle behavior that reintroduces far-behind swarm enemies through an offscreen pressure band
- one-shot major-threat support call-ins when elites, minibosses, or bosses drop below their tuned health threshold
- ground-marked major-threat slam attacks for dread elites, minibosses, and bosses
- kill-streak reward drops and temporary Tempo Surge stat spikes

`Runtime/SurvivorsUiTheme.cs` contains the small JSON-backed presentation theme for player UI:

- rarity display names, style tokens, accent colors, and placeholder icon IDs
- category display names for New Weapon, Weapon Upgrade, Passive, Passive Upgrade, Mutation, Evolution, Relic, Pickup/Magnet, and Meta/Reward
- run-mode display names and common draft/build/result/tutorial/debug button copy
- HUD accent, button style token, and audio event volume/throttle palette
- built-in defaults so programmatic tests and unthemed sample hosts still render readable UI

`Runtime/SurvivorsRunFlow.cs` contains local Survivors run-structure logic:

- elapsed-time phase progression
- escalation multipliers for swarm pressure
- timed enemy role selection for swarm, runner, bruiser, spitter, elite, horde-rush rings, miniboss, and boss pressure
- miniboss and boss profile definitions
- scheduled miniboss/boss spawn events
- boss-defeat and survival-duration victory triggers
- post-victory endless intervals for recurring elite, miniboss, and boss pressure

`Runtime/SurvivorsAuthoredContent.cs` binds the sample's editable gameplay JSON into runtime definitions used by the sample scene:

- authored weapons, projectiles, run-upgrade definitions, explicit effect amounts, rank metadata, draft-card metadata, progression-derived live class gates, evolutions, pickup/magnet passives, relics, classes, one-ability starter loadouts, and bound progression tracks; weapon-track tier/cost data remains validated tooling/reference metadata rather than a live point-spending system
- authored enemy IDs, display names, roles, numeric combat stats, lifecycle flags, marker styles, and major-threat life-bar flags, scaled by the selected authored run-flow profile
- authored Human Playtest, Sprint Run, Normal, Debug Fast, and Showcase profile values for run-mode copy, enemy stat baselines, pacing, XP curves, draft throttles, rarity tables, horde pressure, roaming rewards, and endless surge rewards
- required `sharedGameplayTuning` plus optional non-zero profile overrides for player/dash values, spawn geometry and crowd spacing, threat warnings/slams/support, pickup/reward feedback, waystones, statuses, barriers, and surge feedback; omitted overrides inherit the authored shared value
- authored currencies, meta upgrades, elite/miniboss/boss/class-unlock rewards, reward amounts, and legacy-XP tracks
- strict binding that takes required values directly from JSON and explicit validation failures when gameplay scalars, enemy roles, run-flow profiles, theme/audio/tutorial entries, references, or major reward IDs are missing

`Runtime/SurvivorsWeaponArchetypes.cs` contains the local Survivors weapon-kit runtimes:

- projectile auto-fire adapter
- projectile fan/spread adapter for Frost Fan-style weapons
- hitscan beam adapter
- orbit blade contact runtime
- melee slash arc runtime
- burst nova sequence runtime
- targeted burst and burst-echo hooks for Cinder Burst-style mutation routes
- swept projectile hit checks for fast shots crossing enemies between frames
- local projectile modifier behavior for pierce, chain, fork/split, and return/boomerang
- short-lived primitive visuals, Arc Step dash feedback, dodgeable ranged-enemy warning cues, payload hazard Trap Chain XP/pulse payoffs, waystone discovery and Waystone Chain reward beats, roaming arena reward caches with optional special drops, clearable ambush packs, Arena Trial enemy rings, major-enemy pickup caches with cache-pull attraction, and HUD banners for sample-only feedback

`Runtime/SurvivorsPayloadWeapons.cs` contains the local Survivors payload-kit runtimes:

- arcing grenade payloads with fuse timing
- placed trap and mine payloads with arming, lifetime, and proximity trigger logic
- area explosion overlap and primitive explosion visuals
- simple local hazard zones with tick damage

`Runtime/SurvivorsMetaProgression.cs` contains the local Survivors reward and meta-progression adapter:

- run result summary data
- reference-shaped blood shard and legacy XP reward calculation
- reward lookup and fallback bonus definitions for unbound/debug hosts
- persisted meta profile document and v1-to-v4 migration, including the first-time tutorial-seen flag
- authored ranked persistent upgrades for later-run damage, max health, pickup range, XP gain, and reroll capacity when the sample reward library is bound
- selected class and unlocked class persistence

`Runtime/SurvivorsRelicsAndClasses.cs` contains local Survivors reward/class runtime helpers and fallback definitions:

- deterministic relic draft selection and fallback boss relic definitions for unbound hosts
- current-run relic ownership is tracked by the controller so boss relic drafts stop repeating selected relics
- fallback class definitions, class-owned starting weapon loadouts, starting stat modifiers, and class-gated run-upgrade availability rules when no authored class library is bound
- selected/unlocked class library helpers

`Runtime/SurvivorsProgressionAtlas.cs` contains local Survivors progression grouping helpers and fallback definitions:

- compact class passive atlas and weapon skill track descriptors when no authored progression library is bound
- atlas/track nodes that point at real run-upgrade IDs
- class-specific fallback track metadata used by unbound hosts

`Runtime/SurvivorsContentValidation.cs` contains package-local validation for authored sample libraries and runtime catalogs:

- unique weapon and upgrade IDs
- valid weapon archetype names
- valid projectile references from projectile weapons
- valid payload timing/radius values
- valid boss/miniboss enemy roles, spawn times, and combat stats
- valid pickup IDs, display names, and behavior descriptions; pickup attraction tuning is owned by authored run flow
- valid run-upgrade target references, weights, max ranks, and explicit non-zero effect amounts
- valid upgrade draft-card display names, categories, and description text
- valid pickup-build upgrade coverage for pickup range, gem pull speed, and magnet pulse choices
- valid enemy lifecycle authoring for recyclable normals, persistent major threats, offscreen markers, and life-bar flags
- valid reward currency, legacy XP, persistent upgrade, rank cost, and target references
- valid relic IDs, effect kinds, targets, weights, and amounts
- valid class IDs, default class references, exactly-one starting weapon/loadout references for playable sample classes, class-gated upgrade references, unlock reward IDs, and starting stat modifiers
- valid progression track IDs, passive atlas ownership, weapon-track targets, progression node references, point costs, and max-rank promises
- valid run-flow display copy, required shared gameplay tuning, optional profile overrides, profile enemy stat baselines, pacing values, slot limits, rarity tables, Sprint XP/draft throttles, enemy leash/recycle bands, major-threat catch-up/reposition markers, pickup magnet pulse intervals, endless intervals, Endless Surge rewards, Arena Trial rewards, and ranged-shot dodge XP rewards

This is reusable Survivors template-kit code, not concrete product content.

## Sample Content

`Samples~/BasicSurvivorsGame` contains the package source for the concrete sample game. Manual branch playtesting should use the imported host scene at `Assets/Samples/com.deucarian.template.game.survivors/Basic Survivors Game/Scenes/PLAYTEST_THIS_SCENE_Survivors_Game.unity`.

- `Scenes/BasicSurvivorsGame.unity`
- `Scripts/BasicSurvivorsGameBootstrap.cs`
- `Content/DefaultEnemies/enemies.json`
- `Content/DefaultWeapons/weapons.json`
- `Content/DefaultUpgrades/upgrades.json`
- `Content/DefaultPickups/pickups.json`
- `Content/DefaultRewards/rewards.json`
- `Content/DefaultRelics/relics.json`
- `Content/DefaultClasses/classes.json`
- `Content/DefaultProgression/progression.json`
- `Content/DefaultRunFlow/run-flow.json`
- `Content/DefaultUiTheme/ui-theme.json`
- `Content/NeonArcanaUiTheme/ui-theme.json`

The Basic scene bootstrap strictly binds `Content/DefaultWeapons/weapons.json`, `Content/DefaultUpgrades/upgrades.json`, `Content/DefaultRelics/relics.json`, `Content/DefaultClasses/classes.json`, `Content/DefaultProgression/progression.json`, `Content/DefaultEnemies/enemies.json`, `Content/DefaultRunFlow/run-flow.json`, `Content/DefaultRewards/rewards.json`, `Content/DefaultPickups/pickups.json`, `Content/DefaultUiTheme/ui-theme.json`, and `Content/NeonArcanaUiTheme/ui-theme.json` into the runtime controller before opening the run-mode selector. `Scenes/NeonArcana.unity` uses the same bootstrap type but binds the parallel complete tree under `Content/NeonArcana`, including both alternate themes. Asset-flip authors can change weapon roster/stats, starting ability by class, weapon unlocks, weapon upgrade paths, evolutions, passives, pickup/magnet upgrades, relics, classes, progression tracks and class gates, boss/elite IDs, lifecycle/life-bar flags, run timing, shared/profile tuning, draft pacing, rarity tables, reward definitions, draft-card labels, presentation tokens, tutorial/result copy, HUD accent, world palette, and audio events in those JSON files and then rerun content validation without editing gameplay code.

The sample includes basic swarm, runner, bruiser, ranged spitter with dodgeable shot wind-up and authored dodge XP rewards, splitter and summoner pressure, elite, dread elite, scheduled miniboss, and final boss entries with lifecycle flags for recycle/leash/reposition/marker/life-bar behavior; XP, magnet, vital-shard, and blood-shard pickup entries with Arc Step dash tuning, Gem Rush pickup-cluster stat boosts, waystone discovery and Waystone Chain rewards, roaming arena caches, roaming cache special-drop/ambush tuning, Arena Trial risk/reward tuning, Standard and Sprint run-flow profiles, Sprint XP/draft throttle fields, normal enemy recycle bands, major-threat catch-up/reposition marker values, major-enemy support call-ins, major-enemy slam timing/ground telegraphs, major-enemy reward caches with cache-pull attraction, streak reward banners, Tempo Surge stat spikes, and Endless Surge post-victory major-threat rewards; blood shards; legacy XP; seven persistent upgrades; elite/miniboss/final-boss reward definitions plus a concrete first-clear class-unlock reward; six boss relics chained after miniboss upgrade rewards; a default class with one authored Arc Bolt starter; one-rank weapon unlock definitions for every authored weapon; deeper Frost Fan, Blood Ring, Thorn Halo, Cinder Burst, and Star Beam rank/mutation/evolution paths including Serrated Orbit and Bramble Guard branches; public Star Beam and Gravity Grenade rank/mutation/evolution paths; one unlockable class with one authored Star Beam starter, an advanced stat profile, class-gated Moon Slash rank/mutation/evolution path, and class-gated Rune Trap/Aether Mine rank/mutation/evolution path; Gemheart, Lodestone Sigil, and Vacuum Pulse pickup-build choices; draftable XP-gain and area-scaling passives; Epic upgrade spikes; normal/elite/boss rarity-weight tables; compact passive atlases; weapon skill tracks; explicit upgrade weights/max ranks/effect amounts/requirements; and progression-derived class-gated upgrade metadata. These files are examples for product-owned content flipping. They are not intended to become shared package code.

The Basic Survivors sample scene, authored content, tuning, UI, themes, audio palette, tutorial copy, and loop stay in this template unless an equivalent replacement is wired before extraction. Specific Survivors weapons, enemies, bosses, evolutions, and balance remain product content, not shared package infrastructure.

## Tests

- `Tests/EditMode` covers descriptors, archetype config, expanded enemy profiles, strict authored content binding, missing-field rejection, identifiable fallback hosts, authored one-starter class loadouts, weapon/enemy/upgrade/run-flow/reward mutations, all-field authored tuning parity across five profiles, progression-derived class gates, live weapon/evolution track mutation, default/Neon Arcana theme and audio/tutorial loading, enemy lifecycle authoring, barrier/status behavior, pickup validation/build coverage, reward/meta validation, persistence, payload config, Standard/Sprint separation, draft determinism, spawn flow, weapon death/drop, XP collection, upgrade hooks, and magnet recall.
- `Tests/PlayMode` covers first playable runtime boot, run-mode selection for Standard and Sprint with exactly one starting weapon and matching Current Build HUD, tutorial first-run pause/skip/persistence, theme selection/fallback, audio event routing/mute/throttle, small-screen panel rect smoke, clean player HUD/debug overlay toggle, full-screen draft-card summaries and hotkey selection, comparison previews for numeric stats, weapon unlocks, pickup/magnet upgrades, and evolutions, build menu current-build/stats/run-info/control tabs, top-center timer visibility, exact Sprint 01:42 pacing regression, death/restart, Human Playtest defaults, Sprint first-draft timing, Sprint 5-minute level/draft envelope, Sprint standing-still danger, pickup/magnet build collection behavior without draft flood, Sprint evolution eligibility assist, Arc Step dash movement/shove/safety/cooldown behavior, crowded swarm spacing while maintaining player pressure, normal enemy recycle bands, persistent major-threat reposition/marker/health behavior, removed player-centered placeholder ring/bar visuals, selected class loadouts, owned weapon unlock suppression, class-gated weapon unlock availability, draftable weapon unlocks, full-weapon Arsenal Surge payoff, full-passive Harmony Surge payoff, atlas-derived class-gated upgrade availability, class-gated Moon Slash and placed-hazard rank/evolution eligibility, default reward choices waiting for the player, normal level-up Level Pulse payoff, low-health Clutch Pulse safety and major-threat preservation, multi-evolution Legend Surge payoff, empty level-up draft fallback, timed level-up/relic auto-pick when configured, enemy hit flashes, enemy death bursts, ranged enemy shot cues and wind-up dodging, health pickup recovery, blood-shard pickup currency, Gem Rush pickup-cluster stat boosts, waystone discovery and Waystone Chain payoff beats, roaming arena reward caches with special-drop, ambush, clear-payoff, and Arena Trial Shrine Surge beats, horde-rush warnings, arena bursts, and clear-reward caches, major reward-cache beacons, cache-pull pickup bursts, health readouts, concurrent major-threat life bars, one-shot major-threat support call-ins, and ground-marked major-threat slams, rarity-card presentation and selected reward banners, Arcane Storm, Blizzard Crown, and Inferno Heart evolution behavior, evolution-ready feedback, evolution surge damage, evolution XP recall, and explicit weapon-evolution feedback, orbit damage, melee/burst damage, hitscan damage, projectile sweep reliability, projectile pierce/chain/fork/return smoke, grenade payload damage, placed trap trigger damage, splitter child spawns, elite reward drafts, miniboss upgrade-to-relic reward chaining, timed miniboss/boss spawning, boss/miniboss death, relic choice/application and Relic Surge payoff, Run Summary 2.0 victory/defeat fields, endless continuation with recurring major threats and Endless Surge payoff, reward grants, class unlock persistence, save/load persistence, no-reset persistence behavior for play start/profile application/restart, explicit reset behavior, persistent upgrade effects, XP-gain and area-scaling passives, early/boss rarity weighting, and run upgrades affecting new archetypes/modifiers.
- `Tests/EditMode` also checks the Template Contract and package extraction notes keep the required vertical-slice preservation language and candidate boundaries present.
