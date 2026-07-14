# Deucarian Template Game - Survivors

Playable Unity template package for a Survivors-style horde roguelite loop. The sample boots into a top-down arena with radial enemy spawning, readable horde crowd spacing, timed horde-rush pressure beats with breaker-pulse clear rewards, auto-attacks, XP gems, a short Arc Step dash for emergency spacing, rarity-tinted reward cards, level-up choices with resume pulses, readable damage feedback, enemy hit flashes, death bursts, dodgeable ranged enemy shot cues with small dodge XP rewards, streak reward banners, temporary streak surges, full-weapon Arsenal Surge payoffs, full-passive Harmony Surge payoffs, evolution-ready banners, weapon-evolution surge-and-recall payoffs, multi-evolution Legend Surge payoffs, Rift Caller support-call banners, splitter fragment banners, a top-center run timer, concurrent major-threat life bars and offscreen edge markers, low-health major-threat support call-ins, ground-marked major-threat slam attacks, incoming horde and major-threat warning rings, roaming Arena Trial risk/reward rings, waystone compass HUD guidance, in-world waystone direction arrows, major-reward cache beacons, cache-pull pickup bursts, reward-pick banners, low-health warnings with a one-shot Clutch Pulse, next-milestone HUD countdowns, major-threat telegraphs, miniboss and boss pressure, elite upgrade rewards, boss evolution rewards, boss relic surge rewards, stacking Endless Surge payoffs for post-victory major threats, class unlocks, persistent meta progression, result-screen meta purchases, result-screen class selection, victory/defeat run summaries, endless continuation, and restart flow.

The `Basic Survivors Game` sample is tuned as a small complete template game rather than a toy package demo: each class starts with exactly one authored weapon, every other authored weapon has a draftable unlock definition for leaner class/loadout flips, the horde grows through ten enemy roles including splitter pressure, support-calling Rift Callers, and two reward-bearing elite variants, the level-up pool includes 30+ authored choices with weapon unlocks, behavior mutations, passives, status effects, and eight legendary evolutions, class passive atlases group class identity, weapon skill tracks group upgrade routes, and the run paces toward a 30-minute clear with continuing escalation.

The template is intentionally a game slice, not a reusable Survivors framework. Genre-specific systems stay local until a second concrete game proves a shared package boundary.

## Template Contract

This is a playable game template, not a hollow framework. Import/open the `Basic Survivors Game` sample, press Play, choose Standard / Human Playtest or Sprint Run, and play the vertical slice.

Asset flippers customize authored content in `Samples~/BasicSurvivorsGame/Content` first: weapons, upgrades, evolutions, enemies, run-flow profiles, rewards, progression, classes, themes, audio palette, tutorial copy, and presentation tokens. The template consumes reusable Deucarian packages while keeping its sample scene, authored content, tuning, UI, theme/audio/tutorial content, and playable loop intact.

The same imported sample also contains `Scenes/NeonArcana.unity`, a complete alternate authored game variation backed by `Content/NeonArcana`. It reuses the same bootstrap and gameplay runtime while replacing player-facing weapons, upgrades, enemies, bosses, classes, relics, rewards, progression, mode copy, tutorial, themes, audio-event palette, projectile tint, and world colors. See `Documentation~/neon-arcana-asset-flip.md` for the concrete proof and replacement map.

Package extraction is governed by `Documentation~/template-contract.md`: “Extract only reusable infrastructure, never the playable vertical slice.” Candidate boundaries are tracked in `Documentation~/survivors-package-extraction-candidates.md`.

## Game Content Authoring

After importing the `Basic Survivors Game` sample, open `Tools > Deucarian > Game Content Authoring` and choose `Basic Survivors` or `Neon Arcana` from the global Content Pack selector. Pack Dashboard and All Content expose all 251 records in that selected pack. The reusable Attacks, Enemies, Wave / Encounter, Weapon / Tower, and Upgrades lenses then show the same canonical records through typed, editor-only projections.

Each authored weapon appears as one record in both Attacks and Weapon / Tower; enemy role categories such as Elite and Boss remain semantic views of one Enemy record; run profiles project into Wave / Encounter; and upgrade capabilities drive Weapon Upgrade, Passive, Pickup / Magnet, Mutation, Evolution, and Meta Upgrade filters. Basic and Neon retain separate owner/pack/source/record keys, so references cannot silently cross between them.

Pack Dashboard actions open the selected scene, start its actual strict 30-minute Standard / Human Playtest profile, start its actual strict 5-minute Sprint profile, validate selected `TextAsset` references, reveal the manifest, or open Package Installer. If the sample is missing, the pack reports a missing source and directs installation to Package Installer; Game Content Authoring does not copy or synchronize samples.

JSON remains the only gameplay source of truth. For project-owned imported copies under `Assets/Samples`, the shared edit workbench can safely patch approved direct scalar fields on weapon, projectile, and enemy records, one canonical evolution prerequisite, and exactly one ordered collection: a tutorial step's `Lines`. Tutorial lines support Add, Remove, Move, Replace, Restore Original Order, Undo, and Redo with 1-3 nonblank strings; duplicate text is allowed and displayed order is significant. Stable IDs, archetypes/roles, every other reference or collection, other structural fields, record creation/duplication/deletion, and pack cloning remain read-only. The backend replaces only selected scalar tokens or the selected direct `lines` array span, validates the complete strict pack before save, refuses stale sources, writes atomically, and retains exact recovery bytes under `Library/Deucarian/GameContentAuthoring/Recovery/Survivors`. It never creates a ScriptableObject mirror or round-trips a full JSON DTO. See `Documentation~/game-content-authoring.md` for the exact fields, imported-copy warning, cancellation, rollback, and recovery behavior.

## When To Use This

Use this package when you want:

- A Deucarian-shaped Survivors starter scene that can be opened and played quickly.
- A reference for composing Combat, World Spawning, Weapon Systems, Projectiles, Attacks, Encounters, Run Upgrades, Progression, Persistence, Game Content Authoring, Gameplay Foundation, and Common in a template game.
- Local examples of projectile, projectile fan, orbit, melee, burst, hitscan, grenade, trap, mine, status, barrier, relic, class, passive-atlas, weapon-track, meta, and boss-flow adapters that are safe to customize per game.

Do not use this as a generic Survivors framework, reusable combat rules package, shared projectile package, registry source of truth, or package installer surface. Those capabilities belong to lower reusable packages or other Deucarian owners.

## Install

Unity compatibility: `6000.3` or newer.

Install from Unity Package Manager with one of these Git URLs:

```json
"com.deucarian.template.game.survivors": "https://github.com/Deucarian/Template-Game-Survivors.git#main"
```

```json
"com.deucarian.template.game.survivors": "https://github.com/Deucarian/Template-Game-Survivors.git#develop"
```

Use `#main` for stable package consumption and `#develop` when testing active package work.

## Play In 3 Minutes

1. Add the package through Package Manager.
2. Import the `Basic Survivors Game` sample.
3. Open `Basic Survivors Game/Scenes/BasicSurvivorsGame.unity` under the current imported Survivors sample in `Assets/Samples`.
4. Press Play, then choose Standard Run with `1`/Enter or Sprint Run with `2`/`S`.
5. Move through the horde, collect XP gems, choose level-up options, take elite and miniboss rewards, then build toward a boss evolution reward before defeating the final boss; if no evolution is ready, the boss falls back to a stronger reward draft before victory. Standard Run keeps the 30-minute arc and endless continuation, while Sprint Run compresses the same loop into a 5-minute reward/restart flow. The first clear unlocks Ember Vanguard and pays its authored class-unlock reward into the run summary. Spend earned shards on affordable meta upgrades from the result screen, pick any unlocked next-run class, then continue into endless escalation or restart.

To test the authored flip instead, open `Basic Survivors Game/Scenes/NeonArcana.unity` under the same current imported sample. It offers Neon Arcana Standard with `1` and Neon Arcana Sprint with `2`, using only the alternate content tree.

For local human playtesting of this branch, open `C:\Repositories\Template-Game-Survivors-Playtest` and then open `Basic Survivors Game/Scenes/BasicSurvivorsGame.unity` under its current imported Survivors sample. More detail lives in `Documentation~/playtesting.md`.

The scene contains a tiny bootstrap object. Before opening the run-mode selector, it strictly validates and binds the authored weapon, upgrade, relic, class, progression, enemy, run-flow, reward, pickup, default-theme, and alternate-theme JSON assets. A strict binding error keeps the sample in its pre-run state and shows the actionable validation message instead of starting a fallback-balanced game. Once binding succeeds, the bootstrap creates the selector, arena, player, enemy/pickup/projectile pools, camera, top-center run timer, HUD, drafts, relic UI, meta profile service, result summary, endless continuation, and restart flow.

## Controls

- WASD or arrow keys: move
- Space: Arc Step dash for short emergency spacing, brief safety, and nearby enemy shove
- Pre-run selector: `1`/Enter starts Standard Run; `2`/`S` starts Sprint Run
- Mouse: choose level-up, elite reward, boss evolution, boss relic, result-screen meta upgrade, or next-run class buttons
- 1/2/3: choose level-up, elite reward, boss evolution, boss relic, or affordable result-screen meta upgrade options
- C: continue into endless escalation after victory
- R: reroll the current upgrade draft while charges remain; after death or victory, restart
- S: skip the current upgrade draft for a small blood shard bonus
- Shift+1/2/3: banish an upgrade draft option while charges remain
- M: trigger debug magnet recall

Weapons auto-fire toward nearby targets. XP gems pull toward the player when close, pulse as they are attracted, rapid gem clusters flash Gem Rush pickup banners and briefly improve damage, cooldown, movement, and pickup attraction, existing gems respond immediately to pickup-range upgrades, and Arc Step lets the player punch out of a crowded pocket with a brief safety window plus a small shove/damage pulse against enemies along the path. Gemheart, Lodestone Sigil, and Vacuum Pulse form an authored pickup build path for wider pickup reach, faster gem pull, and periodic XP recall without bypassing the level-up queue. The first damaging hit that pushes the player into low health fires a one-shot Clutch Pulse, clipping nearby regular enemies while granting a brief safety window so the player can reposition without removing elite pressure. The arena uses recentering floor tiles and colored waystones so long travel still has orientation marks instead of feeling like a blank mat; the previous player-centered circle and four bar-like placeholder markers are removed from normal play. Reaching a fresh waystone drops XP, starts a short Focus buff, and can add shard rewards or a small ambush. Repeated fresh waystones now trigger Waystone Chain payoffs with bonus XP, a nearby non-major pulse, and short damage/cooldown/movement/pickup momentum. Sustained travel through the endless arena drops small roaming XP caches with periodic magnet recalls, shard bonuses, clearable trailing ambushes that can pay extra magnet/shard rewards, deeper Wayfinder Surge caches that pay extra XP, and periodic Arena Trials that surround the route with a shrine ring; clearing the ring drops XP/shards, bursts nearby non-major enemies, and briefly improves damage, cooldown, movement, and pickup attraction. After victory, continued exploration scales these waystone, cache, ambush, and shrine rewards with the current Endless tier so travel remains valuable between recurring major threats. Kill streaks call out bonus XP, vital shards, magnet recalls, and blood-shard drops with short banners, sustained streak milestones trigger short Tempo Surges with similar combat momentum, vital shards restore health when wounded, blood shards add run bonus currency for meta progression, result screens let the player switch to unlocked next-run classes, and magnet pickups or the debug recall make distant XP gems spin and surge inward with stronger pickup feedback.

The default playable kit recreates the spirit of the reference clone's Arc Bolt, Frost Fan, Blood Ring, Thorn Halo, and Cinder Burst weapons while keeping stable Deucarian template IDs. Every authored weapon now has a one-rank weapon unlock entry, and owned weapon unlocks are hidden from the live draft so asset flips can start with fewer weapons without offering upgrades for missing weapons. Filling the weapon slots triggers Arsenal Surge, a one-shot pulse and short combat-momentum boost that makes the sixth weapon feel like a milestone; filling passive slots triggers Harmony Surge, a nearby regular-enemy pulse plus temporary damage, cooldown, movement, pickup, and XP-gain momentum. Upgrade choices then add poison, bleed, burn, execute, lifesteal, barrier, XP gain, area scaling, fan spread, pierce, chain, fork, return, orbit wall, targeted burst, burst echo, ranked payload growth, payload mutations, passive build requirements, Epic build spikes, and legendary weapon evolutions. Arc Bolt can evolve into Arcane Storm, adding a radial bolt ring to each shot; Frost Fan can evolve into Blizzard Crown, adding a radial shard crown around the player; Cinder Burst now burns enemies on hit and evolves into Inferno Heart, adding satellite novas plus stronger burn pressure around each burst. The Blood Ring/Thorn Halo orbit lane, Star Beam, and Gravity Grenade now have normal rank paths before their branch/evolution payoff: Blood Ring ranks Orbiting Focus into Serrated Orbit or Blood Ring Canticle before Crimson Aegis adds a counter-rotating shield ring that pushes regular enemies back, Thorn Halo ranks Bramble Guard into Thorn Halo Wall or Halo Spiral, Star Beam ranks Star Focus into Prismatic Beam or Star Pulse before Tempest Prism splits the beam into angled prism rays with arcing refractions, and Gravity Grenade grows through Extra Payload into Bigger Booms or Wider Triggers before Gravefield Engine adds snaring satellite danger fields. Ember Vanguard also has class-gated Moon Slash and placed-hazard routes: Moonlit Edge branches into Crescent Chain or Lunar Tempo before Eclipse Waltz adds a backward sweep, and Rune Lattice branches into Snaring Runes or Aether Bloom before Aetherfield Matrix adds snaring satellite hazard fields. Hazard fields now build toward Trap Chain moments when enough enemies are snared together, dropping bonus XP and pulsing nearby non-major enemies so the payload lane has a readable clump-control payoff. When a maxed weapon path and its matching passive make an evolution eligible, the HUD flashes an Evolution Ready banner so players know the next elite or boss reward can pay off the build; selecting an evolution also fires a tuned surge that damages nearby non-major enemies and recalls loose XP gems so the power spike is immediate. The second and later evolved weapons trigger a short Legend Surge, adding another nearby non-major pulse plus temporary damage, cooldown, movement, and pickup attraction so late builds feel like they are compounding. Normal, elite, and boss drafts use separate rarity-weight tables so early level-ups lean Common/Uncommon while boss rewards prefer Rare/Epic/Legendary choices; normal level-up picks fire a smaller Level Pulse as play resumes, elite and boss rewards lock a reward-tier first card when no evolution is ready, and ready evolutions still lead the reward draft. Their cards show rarity/category at a glance, major foes burst into XP and special-pickup caches when killed, selected fallback rewards fire a tuned Reward Surge through nearby non-major enemies, Rare-or-better fallback reward picks spill a pulled-in jackpot of XP and blood shards, and selected rewards briefly flash as a banner after the run resumes. Human Playtest leaves level-up and reward choices open until the player chooses; faster validation profiles can use timed auto-pick.

Boss relic drafts are current-run collection picks: once a relic is selected, later boss relic drafts remove it from the candidate pool, and the reward chain resumes normally if every relic has already been claimed. Picking one emits a short Relic Surge through nearby non-major enemies, briefly improves damage, cooldown, movement, and pickup attraction, and the build HUD and run summary include the current relic count so these miniboss rewards remain visible after the selection banner fades.

If the player has not claimed a passive yet, normal level-up drafts reserve a Common/Uncommon passive hook before filling the rest of the cards normally. Once a weapon path reaches an evolution requirement but the matching passive is still missing, the HUD flashes an Evolution Goal banner, keeps the missing-passive goal pinned in the build HUD, and normal drafts also reserve that required passive when the current rarity table can offer it. When the evolution is fully eligible, the build HUD switches to a ready prompt that points the player toward elite/boss rewards. This helps build identity start forming in the opening drafts and helps ready builds reach elite/boss evolution rewards without bypassing rarity tuning.

Standard Run uses `SurvivorsPacingProfile.HumanPlaytest`: `Time.timeScale` is reset to `1`, opening enemy spawns use two-enemy packs on a `0.95` second interval, pack size grows with escalation up to `6`, the opening max-alive cap is `38`, basic enemy speed is `1.35` versus player speed `5.55`, nearby enemies nudge apart so packs stay readable while still pressing inward, reward auto-pick is off, horde-rush events warn and then spawn a mixed enemy ring between major threats, clearing the ring drops a bonus cache, fires a breaker pulse into nearby regular enemies, and starts a short Breaker Surge for damage, cooldown, movement, and pickup attraction, Rift Callers enter the regular horde around `155` seconds and periodically call nearby support enemies, active elites/minibosses/bosses show a prioritized health bar, far major threats show an offscreen edge marker and safely re-enter without losing health, damaged major enemies call in a one-shot support ring near their low-health threshold, and dread elites/minibosses/bosses paint a brief ground slam disc before damaging nearby players. Normal enemies that fall far behind can recycle through an offscreen band to preserve pressure without instant contact. The first horde rush begins around `75` seconds, the first elite arrives around `135` seconds, the dread elite arrives around `255` seconds, the miniboss waits until `360` seconds, the final boss appears around `1140` seconds, and survival victory lands at `1800` seconds. After victory, continuing schedules recurring elite, miniboss, and boss pressure using the endless interval tuning on `SurvivorsTemplateTuning`; defeating those recurring major threats triggers stacking Endless Surge reward bursts with extra XP, blood shards, a non-major enemy pulse, and temporary damage, cooldown, movement, and pickup momentum.

Sprint Run uses `SurvivorsPacingProfile.SprintRun` for a separate 5-minute loop: opening spawns use a `0.72` second interval with up to `46` alive enemies, denser offscreen recycle bands, a stricter XP curve of `102 + 20 per level`, an `18` second normal level-up draft cooldown, and at most `1` queued level-up draft. Reroll and banish charges start at `5`, the first elite arrives around `82` seconds, the first horde spike starts around `120` seconds, dread pressure appears around `165` seconds, the miniboss appears around `175` seconds, the final boss appears around `270` seconds, and victory lands at `300` seconds. Sprint lowers total meta rewards with a `0.65` reward multiplier, disables endless continuation, and applies a Sprint-only evolution rank assist so a focused build can reach an evolution without changing Standard rules. `Normal`, `DebugFast`, and `Showcase` profiles exist for validation and demos, but must be selected deliberately through the runtime debugger.

## What To Customize First

- Weapon and projectile feel: edit `Samples~/BasicSurvivorsGame/Content/DefaultWeapons/weapons.json`. Strict sample weapons own their visible names, colors, cooldowns, damage, range, and archetype-specific projectile/orbit/melee/burst/hitscan/payload values; `Runtime/BasicSurvivorsGame.cs` weapon definitions are unbound-host/test fallbacks, not a second source for the playable sample.
- Upgrades, evolutions, class gates, passive atlases, and weapon tracks: edit `Samples~/BasicSurvivorsGame/Content/DefaultUpgrades/upgrades.json` and `Samples~/BasicSurvivorsGame/Content/DefaultProgression/progression.json`. Upgrade JSON owns draft-card metadata, rarity, weight, max rank, effects, targets, amounts, and live gameplay prerequisites. Progression JSON owns class passive atlases, weapon tracks/nodes, node rank/cost metadata, evolution nodes, and class-specific upgrade gates. Class-atlas nodes drive strict runtime gates; weapon-track tier/cost data is bound reference/tooling metadata and is not a live point-spending system.
- Enemies, pickups, run flow, and rewards: edit `Samples~/BasicSurvivorsGame/Content/DefaultEnemies/enemies.json`, `Samples~/BasicSurvivorsGame/Content/DefaultPickups/pickups.json`, `Samples~/BasicSurvivorsGame/Content/DefaultRunFlow/run-flow.json`, and `Samples~/BasicSurvivorsGame/Content/DefaultRewards/rewards.json`. Enemy JSON owns identities, roles, combat baselines, lifecycle flags, marker/life-bar behavior, and authored role gates. Pickup JSON is the pickup identity/behavior manifest. Run flow owns the required shared gameplay baseline, optional authored profile overrides, every profile's pacing/pressure/XP/draft/rarity/reward values, pickup attraction, and Standard/Sprint separation. Rewards JSON owns currencies, run grants, legacy-XP grants, and persistent meta upgrades.
- Boss relics: edit `Samples~/BasicSurvivorsGame/Content/DefaultRelics/relics.json` and `CreateRelicDefinitions`.
- Classes and starting loadouts: edit `Samples~/BasicSurvivorsGame/Content/DefaultClasses/classes.json` and `CreateClassLibraryDefinition`.
- Run tuning, pacing profiles, player/dash values, spawn geometry, orbit knockback, horde/crowd pressure, threat warning/slam/support, Gem Rush, level/reward/evolution/relic/loadout surges, roaming caches, Arena Trials, waystones, status durations, pickup values, XP/draft throttles, rarity weights, and endless rewards: edit `Samples~/BasicSurvivorsGame/Content/DefaultRunFlow/run-flow.json`. Shared values belong in `sharedGameplayTuning`; only deliberate profile differences belong in `gameplayTuningOverrides`. `CreateDefaultTuning` remains a fallback/parity reference, not the normal sample authoring surface.
- Debug iteration: use `Tools > Deucarian > Templates > Survivors > Runtime Debugger` during Play Mode to start Standard or Sprint, grant XP or blood shards, force level-ups, trigger or clear horde rushes, force elite/miniboss/boss spawns including a Sprint boss, spawn bursts, fill the arena, switch pacing profiles with a restart, inspect live build ranks/evolutions/drafts with current-to-next rank labels, inspect run metrics, trigger magnet recall, or reset meta progression.

## Asset Flip Shape

Future games should mostly change sample content and template-local catalog defaults:

- weapons, weapon names, projectile fan/spread values, orbit radius/count, payload tuning, and burst behavior
- enemies, role timing, pressure profiles, boss stats, and reward values
- upgrades, evolutions, status/sustain/barrier/mutation effects, class gates, draft weights, and draft quality-of-life tuning
- classes, starting loadouts, passive atlases, weapon skill tracks, stat modifiers, unlock rewards, relics, and meta upgrade costs
- visuals, colors, audio clips, arena primitives, and authored sample docs

The normal Basic sample takes asset-flippable values from `Samples~/BasicSurvivorsGame/Content` under `SurvivorsAuthoredContentBindingPolicy.StrictSample`. `Runtime/BasicSurvivorsGame.cs` remains a deliberately identifiable fallback catalog for tests, debug tools, and hosts that do not bind a complete authored sample. Runtime code still owns algorithms such as deterministic selection, collision/tick execution, clamping, pooling, rendering, and scaling authored enemy baselines between authored profiles.

## Gameplay Parity With Reference Clone

The reference clone has a much larger authored project: class-owned content packs, passive atlases, per-weapon skill trees, boss relic pools, damage augments, enemy variety, exponential run bloat, save tooling, and runtime debug controls. This package now brings over the template-sized parts that matter most for pressing Play:

- five immediately active clone-spirit weapons instead of a single starter wand
- ten enemy pressure types with splitters that show fragment banners, ranged pressure, regular support-calling Rift Callers, timed horde-rush rings, low-health support call-ins for major threats, two elite variants, and long-run escalation instead of one swarm profile
- 30+ meaningful upgrade choices with visible behavior changes, status/sustain hooks, passive requirements, Evolution Ready cues, Epic spikes, and eight legendary evolutions
- elite kill reward drafts, miniboss rewards, boss evolution rewards with rare-or-better fallback drafts, boss relic choices, multiple persistent meta upgrades, class loadouts, compact passive atlases, draftable weapon unlock tracks, and class-gated advanced tools
- reroll, skip-for-shards, and banish controls for draft quality of life
- reward-choice timeout behavior so long sessions keep moving if a choice is left open
- runtime debug controls under Deucarian menu conventions

Intentionally not ported wholesale: the clone's twelve-class content-pack ecosystem, Odin authoring workflow, full passive graph UI, per-weapon graph editor, element/resistance matrix, and class resource economies. The template keeps a compact local atlas/track shape instead of the product-specific graph tooling until another template/game proves the reusable boundary.

## Sample And API Map

- Manual playtest scene: `Basic Survivors Game/Scenes/BasicSurvivorsGame.unity` under the current imported Survivors sample in `Assets/Samples`
- Package source scene, not the branch manual-test target: `Samples~/BasicSurvivorsGame/Scenes/BasicSurvivorsGame.unity`
- Sample bootstrap: `Samples~/BasicSurvivorsGame/Scripts/BasicSurvivorsGameBootstrap.cs`
- Main runtime catalog: `Runtime/BasicSurvivorsGame.cs`
- Local progression atlas model: `Runtime/SurvivorsProgressionAtlas.cs`
- Runtime controller: `Runtime/SurvivorsTemplateController.cs`
- Local content validation: `Runtime/SurvivorsContentValidation.cs`
- Editor validation menu: `Editor/SurvivorsEditorContentValidation.cs`
- Structure notes: `Documentation~/survivors-template-structure.md`
- Playtesting notes: `Documentation~/playtesting.md`
- Validation notes: `Documentation~/validation.md`
- Template Contract: `Documentation~/template-contract.md`
- Package extraction candidates: `Documentation~/survivors-package-extraction-candidates.md`

## Integrations

This slice uses:

- `com.deucarian.common` for approved transient Unity object cleanup.
- `com.deucarian.combat` for health and damage resolution.
- `com.deucarian.world-spawning` for pooled enemies, pickups, and projectile visuals.
- `com.deucarian.weapon-systems`, `com.deucarian.projectiles`, and `com.deucarian.attacks` for stable descriptors and package-owned primitives.
- `com.deucarian.run-upgrades` for draft and run-upgrade selection state.
- `com.deucarian.encounters` for authored spawn-flow descriptors.
- `com.deucarian.progression` for local meta currency, legacy XP, and ranked persistent upgrade state.
- `com.deucarian.persistence` for the local Survivors meta profile save document.
- `com.deucarian.game-content-authoring` from editor validation code for report formatting.
- `com.deucarian.gameplay-foundation` for stable IDs and deterministic primitives surfaced by Deucarian runtime packages.

## Local Template Code

Keep these systems local to this template until reuse is proven across another Survivors-style game:

- Player movement, camera feel, radial spawn pose rules, run timing, escalation, boss/miniboss scheduling, and victory state.
- Elite reward rules, boss evolution reward rules, boss relic choice rules, draft reroll/skip/banish rules, class selection/unlock rules, class starting loadouts, class stat modifiers, and class upgrade gates.
- Class passive atlas and weapon skill track grouping rules that derive class-specific upgrade availability.
- Run summary data, meta upgrade effects, XP magnet behavior, level-up/relic HUD, and concrete projectile behavior.
- Hitscan targeting, beam visuals, projectile modifier rules, payload placement, detonation, hazard rules, orbit motion, melee arc overlap, and burst nova timing.
- Survivors-specific content validation rules for sample JSON and runtime catalogs.

## Validation

### Content Validation

From the Unity editor, run:

`Tools > Deucarian > Templates > Survivors > Validate Content`

The menu validates strict sample JSON and runtime catalogs for required scalar/display fields, IDs, archetype/projectile references, upgrade targets and amounts, pickup manifests, pickup-build coverage, weapon roles, enemy combat/lifecycle/marker fields, boss/miniboss definitions, relics, class starters, progression atlases/tracks/nodes/gates, the shared gameplay-tuning baseline and profile overrides, Sprint XP/draft throttles, rewards, themes, audio IDs, tutorial copy, and unlock requirements. The report is written to the Unity console for template developer visibility.

Before committing package changes, run:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
git diff --check
```

Run existing Unity EditMode and PlayMode tests when changing code, asmdefs, package dependencies, sample JSON, or sample behavior.

During Play Mode, run:

`Tools > Deucarian > Templates > Survivors > Runtime Debugger`

Use it to force XP, grant blood shards, force level-up, trigger or clear horde rushes, force elite/miniboss/boss spawns, spawn enemy bursts, fill the arena, apply stress targets, switch pacing profiles with a restart, inspect live spawn interval/pack/counts, horde-rush and threat-enrage state, build ranks, eligible evolutions, and the current draft pool, trigger magnet recall, and reset the local meta profile.

## Screenshots And GIFs

No screenshot or GIF assets are committed yet. Add `Documentation~/media/` captures once the sample has stable visual direction, then link the first gameplay GIF, one class/relic choice screenshot, and one boss-wave screenshot from this section.

## Troubleshooting

- Sample scene is missing: import `Basic Survivors Game` through Package Installer or Package Manager, then open `Basic Survivors Game/Scenes/BasicSurvivorsGame.unity` under the current import in `Assets/Samples`.
- Mode cards are disabled with an authored-content error: run `Tools > Deucarian > Templates > Survivors > Validate Content`, fix the named JSON field/reference in the imported sample source, and reimport the sample if its copied scene/content is stale. Strict mode intentionally will not substitute `BasicSurvivorsGame` values.
- Weapons do not fire: enemies must be in range; move near the horde and wait for auto-fire cadence.
- Combat feedback is hard to read: confirm the imported sample is current, then damage enemies or take a hit; enemies should flash on hit, deaths should leave a short burst, ranged attackers should draw a quick hostile shot cue before damage lands, streak drops should show a short reward banner, major reward kills should pop a colored cache beacon and pull their cache pickups inward, resolved damage should appear as short-lived numbers, and the first low-health crossing should pulse the red screen edge plus fire a Clutch Pulse.
- Draft choices do not appear: collect XP gems until the level-up overlay opens, then choose with the mouse or `1`, `2`, or `3`.
- Relic choices do not appear: defeat the miniboss first, then resolve its upgrade reward; the boss relic draft opens immediately after that reward.
- Persistent class or meta state looks stale: use the template reset/debug hooks from tests or clear the local sample save before validating a fresh profile.
- Defeat or victory is stuck: press `R` or click the restart button.

## Deferred

Full graph-editor passive skill trees, per-weapon graph editors, the clone's full class content-pack ecosystem, production UI, monetization, networking, and shared package extraction are deferred.

## License

MIT. See `LICENSE.md`.
