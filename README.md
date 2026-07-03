# Deucarian Template Game - Survivors

Playable Unity template package for a Survivors-style horde roguelite loop. The sample boots into a top-down arena with radial enemy spawning, auto-attacks, XP gems, rarity-tinted reward cards, level-up choices, readable damage feedback, enemy hit flashes, death bursts, ranged enemy shot cues, streak reward banners, temporary streak surges, evolution-ready banners, major-reward cache beacons and pickup bursts, reward-pick banners, low-health warnings, major-threat telegraphs, miniboss and boss pressure, elite upgrade rewards, boss evolution rewards, relic rewards, class unlocks, persistent meta progression, victory, endless continuation, defeat, and restart flow.

The `Basic Survivors Game` sample is tuned as a small complete template game rather than a toy package demo: the default class starts with five distinct weapons, extra weapons such as Star Beam and Gravity Grenade can enter through the draft pool, the horde grows through nine enemy roles including splitter pressure and two reward-bearing elite variants, the level-up pool includes 30+ authored choices with weapon unlocks, behavior mutations, passives, status effects, and eight legendary evolutions, class passive atlases group class identity, weapon skill tracks group upgrade routes, and the run paces toward a 30-minute clear with continuing escalation.

The template is intentionally a game slice, not a reusable Survivors framework. Genre-specific systems stay local until a second concrete game proves a shared package boundary.

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
3. Open the imported sample scene at `Assets/Samples/com.deucarian.template.game.survivors/Basic Survivors Game/Scenes/PLAYTEST_THIS_SCENE_Survivors_Game.unity`.
4. Press Play.
5. Move through the horde, collect XP gems, choose level-up options, take the elite reward after the miniboss, then build toward a boss evolution reward before defeating the final boss; if no evolution is ready, the boss falls back to a stronger reward draft before victory. After victory, continue into endless escalation or restart.

For local human playtesting of this branch, open `C:\Repositories\Template-Game-Survivors-Playtest` and then open `Assets/Samples/com.deucarian.template.game.survivors/Basic Survivors Game/Scenes/PLAYTEST_THIS_SCENE_Survivors_Game.unity`. More detail lives in `Documentation~/playtesting.md`.

The scene contains a tiny bootstrap object. At runtime it creates the arena, player, enemy/pickup/projectile pools, camera, run timer, HUD, draft UI, relic UI, meta profile service, victory state, endless continuation, defeat state, and restart flow.

## Controls

- WASD or arrow keys: move
- Mouse: choose level-up, elite reward, boss evolution, or boss relic buttons
- 1/2/3: choose level-up, elite reward, boss evolution, or boss relic options
- C: continue into endless escalation after victory
- R: reroll the current upgrade draft while charges remain; after death or victory, restart
- S: skip the current upgrade draft for a small blood shard bonus
- Shift+1/2/3: banish an upgrade draft option while charges remain
- M: trigger debug magnet recall

Weapons auto-fire toward nearby targets. XP gems pull toward the player when close, pulse as they are attracted, rapid gem clusters flash Gem Rush pickup banners, existing gems respond immediately to pickup-range upgrades, sustained travel through the endless arena drops small roaming XP caches with periodic magnet recalls, kill streaks call out bonus XP, vital shards, magnet recalls, and blood-shard drops with short banners, sustained streak milestones trigger short Tempo Surges that improve damage, cooldown, movement, and pickup attraction, vital shards restore health when wounded, blood shards add run bonus currency for meta progression, and magnet pickups or the debug recall make distant XP gems spin and surge inward with stronger pickup feedback.

The default playable kit recreates the spirit of the reference clone's Arc Bolt, Frost Fan, Blood Ring, Thorn Halo, and Cinder Burst weapons while keeping stable Deucarian template IDs. Upgrade choices can unlock Star Beam or Gravity Grenade when weapon slots are open, then add poison, bleed, execute, lifesteal, barrier, XP gain, area scaling, fan spread, pierce, chain, fork, return, orbit wall, targeted burst, burst echo, ranked payload growth, payload mutations, passive build requirements, Epic build spikes, and legendary weapon evolutions. Arc Bolt can evolve into Arcane Storm, adding a radial bolt ring to each shot; Frost Fan can evolve into Blizzard Crown, adding a radial shard crown around the player; Cinder Burst can evolve into Inferno Heart, adding satellite novas around each burst. The Blood Ring/Thorn Halo orbit lane, Star Beam, and Gravity Grenade now have normal rank paths before their branch/evolution payoff: Blood Ring ranks Orbiting Focus into Thorn Halo Wall or Halo Spiral before Crimson Aegis adds a counter-rotating shield ring, Star Beam ranks Star Focus into Prismatic Beam or Star Pulse before Tempest Prism splits the beam into angled prism rays, and Gravity Grenade grows through Extra Payload into Bigger Booms or Wider Triggers before Gravefield Engine adds satellite danger fields. Ember Vanguard also has class-gated Moon Slash and placed-hazard routes: Moonlit Edge branches into Crescent Chain or Lunar Tempo before Eclipse Waltz adds a backward sweep, and Rune Lattice branches into Snaring Runes or Aether Bloom before Aetherfield Matrix adds satellite hazard fields. When a maxed weapon path and its matching passive make an evolution eligible, the HUD flashes an Evolution Ready banner so players know the next elite or boss reward can pay off the build. Normal, elite, and boss drafts use separate rarity-weight tables so early level-ups lean Common/Uncommon while boss rewards prefer Rare/Epic/Legendary choices; their cards show rarity/category at a glance, major foes burst into XP and special-pickup caches when killed, and selected rewards briefly flash as a banner after the run resumes. Human Playtest leaves level-up and reward choices open until the player chooses; faster validation profiles can use timed auto-pick.

Default play uses `SurvivorsPacingProfile.HumanPlaytest`: `Time.timeScale` is reset to `1`, opening enemy spawns use two-enemy packs on a `1.15` second interval, pack size grows with escalation up to `5`, the opening max-alive cap is `34`, basic enemy speed is `1.35` versus player speed `5.4`, reward auto-pick is off, major enemies show a center-screen warning before they arrive, the first elite arrives around `180` seconds, the dread elite arrives around `300` seconds, the miniboss waits until `420` seconds, the final boss appears around `1200` seconds, and survival victory lands at `1800` seconds. After victory, continuing schedules recurring elite, miniboss, and boss pressure using the endless interval tuning on `SurvivorsTemplateTuning`. `Normal`, `DebugFast`, and `Showcase` profiles exist for validation and demos, but must be selected deliberately through the runtime debugger.

## What To Customize First

- Weapon and projectile feel: edit `Samples~/BasicSurvivorsGame/Content/DefaultWeapons/weapons.json`, then compare with `Runtime/BasicSurvivorsGame.cs` and `CreateWeaponArchetypeDefinitions`.
- Upgrades, evolutions, class gates, passive atlases, and weapon tracks: edit `Samples~/BasicSurvivorsGame/Content/DefaultUpgrades/upgrades.json`, `Samples~/BasicSurvivorsGame/Content/DefaultProgression/progression.json`, `CreateRunUpgradeCatalog`, `CreateRunUpgradeMetadata`, and `CreateProgressionTrackDefinitions`. Class gates are derived from class-specific progression tracks.
- Enemies, pickups, miniboss, boss, and rewards: edit `Samples~/BasicSurvivorsGame/Content/DefaultEnemies/enemies.json`, `Samples~/BasicSurvivorsGame/Content/DefaultPickups/pickups.json`, `Samples~/BasicSurvivorsGame/Content/DefaultRewards/rewards.json`, and `CreateRunFlowDefinition`.
- Boss relics: edit `Samples~/BasicSurvivorsGame/Content/DefaultRelics/relics.json` and `CreateRelicDefinitions`.
- Classes and starting loadouts: edit `Samples~/BasicSurvivorsGame/Content/DefaultClasses/classes.json` and `CreateClassLibraryDefinition`.
- Run tuning, pacing profiles, horde pack sizes, kill-streak reward/surge pacing, pickup heal/currency amounts, endless major-threat intervals, reward timeout, draft rarity weights, draft rerolls, draft banishes, and skip rewards: start with `CreateDefaultTuning` and `CreateTuning(SurvivorsPacingProfile)` before changing controller internals.
- Debug iteration: use `Tools > Deucarian > Templates > Survivors > Runtime Debugger` during Play Mode to grant XP or blood shards, force level-ups, force elite/miniboss/boss spawns, spawn bursts, fill the arena, switch pacing profiles with a restart, apply stress profiles, inspect live build ranks/evolutions/drafts, trigger magnet recall, or reset meta progression.

## Asset Flip Shape

Future games should mostly change sample content and template-local catalog defaults:

- weapons, weapon names, projectile fan/spread values, orbit radius/count, payload tuning, and burst behavior
- enemies, role timing, pressure profiles, boss stats, and reward values
- upgrades, evolutions, status/sustain/barrier/mutation effects, class gates, draft weights, and draft quality-of-life tuning
- classes, starting loadouts, passive atlases, weapon skill tracks, stat modifiers, unlock rewards, relics, and meta upgrade costs
- visuals, colors, audio clips, arena primitives, and authored sample docs

The template keeps this data split between `Runtime/BasicSurvivorsGame.cs` for playable defaults and `Samples~/BasicSurvivorsGame/Content` JSON for asset-flip authoring examples. The JSON is validated by the editor tool and is intended to become the first place future content authors look.

## Gameplay Parity With Reference Clone

The reference clone has a much larger authored project: class-owned content packs, passive atlases, per-weapon skill trees, boss relic pools, damage augments, enemy variety, exponential run bloat, save tooling, and runtime debug controls. This package now brings over the template-sized parts that matter most for pressing Play:

- five immediately active clone-spirit weapons instead of a single starter wand
- nine enemy pressure types with splitters, ranged pressure, two elite variants, and long-run escalation instead of one swarm profile
- 30+ meaningful upgrade choices with visible behavior changes, status/sustain hooks, passive requirements, Evolution Ready cues, Epic spikes, and eight legendary evolutions
- elite kill reward drafts, miniboss rewards, boss evolution rewards with rare-or-better fallback drafts, boss relic choices, multiple persistent meta upgrades, class loadouts, compact passive atlases, draftable weapon unlock tracks, and class-gated advanced tools
- reroll, skip-for-shards, and banish controls for draft quality of life
- reward-choice timeout behavior so long sessions keep moving if a choice is left open
- runtime debug controls under Deucarian menu conventions

Intentionally not ported wholesale: the clone's twelve-class content-pack ecosystem, Odin authoring workflow, full passive graph UI, per-weapon graph editor, element/resistance matrix, and class resource economies. The template keeps a compact local atlas/track shape instead of the product-specific graph tooling until another template/game proves the reusable boundary.

## Sample And API Map

- Manual playtest scene: `Assets/Samples/com.deucarian.template.game.survivors/Basic Survivors Game/Scenes/PLAYTEST_THIS_SCENE_Survivors_Game.unity`
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

The menu validates sample JSON and runtime catalogs for IDs, archetype references, projectile references, upgrade targets, pickup definitions, boss/miniboss enemy definitions, relic effects, class IDs, starting weapons, passive atlases, weapon skill tracks, and unlock requirements. The report is written to the Unity console for template developer visibility.

Before committing package changes, run:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
git diff --check
```

Run existing Unity EditMode and PlayMode tests when changing code, asmdefs, package dependencies, sample JSON, or sample behavior.

During Play Mode, run:

`Tools > Deucarian > Templates > Survivors > Runtime Debugger`

Use it to force XP, grant blood shards, force level-up, force elite/miniboss/boss spawns, spawn enemy bursts, fill the arena, apply stress targets, switch pacing profiles with a restart, inspect live spawn interval/pack/counts, build ranks, eligible evolutions, and the current draft pool, trigger magnet recall, and reset the local meta profile.

## Screenshots And GIFs

No screenshot or GIF assets are committed yet. Add `Documentation~/media/` captures once the sample has stable visual direction, then link the first gameplay GIF, one class/relic choice screenshot, and one boss-wave screenshot from this section.

## Troubleshooting

- Sample scene is missing: import `Basic Survivors Game` from Package Manager, then open the imported playtest scene at `Assets/Samples/com.deucarian.template.game.survivors/Basic Survivors Game/Scenes/PLAYTEST_THIS_SCENE_Survivors_Game.unity`.
- Weapons do not fire: enemies must be in range; move near the horde and wait for auto-fire cadence.
- Combat feedback is hard to read: confirm the imported sample is current, then damage enemies or take a hit; enemies should flash on hit, deaths should leave a short burst, ranged attackers should draw a quick hostile shot cue, streak drops should show a short reward banner, major reward kills should pop a colored cache beacon and pickup burst, resolved damage should appear as short-lived numbers, and low health should pulse a red screen-edge warning.
- Draft choices do not appear: collect XP gems until the level-up overlay opens, then choose with the mouse or `1`, `2`, or `3`.
- Relic choices do not appear: defeat the miniboss first.
- Persistent class or meta state looks stale: use the template reset/debug hooks from tests or clear the local sample save before validating a fresh profile.
- Defeat or victory is stuck: press `R` or click the restart button.

## Deferred

Full graph-editor passive skill trees, per-weapon graph editors, the clone's full class content-pack ecosystem, production UI, monetization, networking, and shared package extraction are deferred.

## License

MIT. See `LICENSE.md`.
