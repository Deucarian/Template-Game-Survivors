# Neon Arcana Asset-Flip Proof

Neon Arcana is a second complete authored variation inside the `Basic Survivors Game` sample. It proves that the playable vertical slice can change brand, names, content presentation, world palette, enemies, bosses, rewards, classes, tutorial, and audio-event tuning while continuing to use the same `BasicSurvivorsGameBootstrap`, `SurvivorsTemplateController`, weapon executors, run flow, spawning, combat, XP, draft, evolution, reward, persistence, and restart code.

## Launch Both Variations

- Basic Survivors source scene: `Samples~/BasicSurvivorsGame/Scenes/BasicSurvivorsGame.unity`.
- Neon Arcana source scene: `Samples~/BasicSurvivorsGame/Scenes/NeonArcana.unity`.
- After importing the sample, open the matching scene under `Basic Survivors Game/Scenes` in the current Survivors import under `Assets/Samples`.
- Press Play. Both scenes wait at run-mode selection. Press `1` for the 30-minute Standard / Human Playtest profile or `2` for the 5-minute Sprint profile.
- The Neon scene identifies itself with `PLAYTEST_THIS_SCENE_NEON_ARCANA`, binds `Neon Arcana` and `Neon Arcana: Afterglow`, and never references a Basic content GUID.

## Alternate Content Structure

The complete alternate tree is `Samples~/BasicSurvivorsGame/Content/NeonArcana`:

| Area | Authored file |
| --- | --- |
| Weapons, projectile values, visible names, tints | `Weapons/weapons.json` |
| Drafts, passives, pickup builds, mutations, evolutions | `Upgrades/upgrades.json` |
| Swarm roles, elites, dread elite, miniboss, boss | `Enemies/enemies.json` |
| Pickup identities | `Pickups/pickups.json` |
| Relics | `Relics/relics.json` |
| Classes and one-weapon starting loadouts | `Classes/classes.json` |
| Passive atlases and weapon/evolution tracks | `Progression/progression.json` |
| Currency, progression, rewards, meta upgrades | `Rewards/rewards.json` |
| Standard, Sprint, validation profiles, all tuning | `RunFlow/run-flow.json` |
| UI, rarity, cards, tutorial, audio events, world palette | `Themes/NeonArcana/ui-theme.json` and `Themes/Afterglow/ui-theme.json` |

The alternate files intentionally keep the template's stable internal behavior IDs, such as `weapon.survivors.arcane-wand`. Those IDs are runtime contracts, not Basic display content. The Neon scene binds independent JSON assets whose values name that behavior Neon Pulse, Prism Volley, Photon Guard, Reactor Nova, and the rest of the alternate catalog. No JSON, theme, or scene reference falls through to a `Default*` asset.

## What To Replace

- Replace weapon names, tints, timings, projectile values, and behavior-archetype values in `Weapons/weapons.json`.
- Replace draft copy, paths, passives, pickup upgrades, mutations, and evolutions in `Upgrades/upgrades.json` while keeping references valid.
- Replace every role's identity, tint, combat baseline, lifecycle policy, bars, and markers in `Enemies/enemies.json`.
- Replace class names and starter lists in `Classes/classes.json`. A normal asset-flip starter must contain exactly one authored weapon.
- Replace relics, progression labels/nodes, currencies, run rewards, and meta-upgrade presentation in their dedicated files.
- Replace mode display names and descriptions in `RunFlow/run-flow.json`. Pacing remains separate from theme copy.
- Replace UI copy, rarity/category labels, icon IDs, tutorial, audio event volume/throttle values, and `worldPresentation` colors in a theme file.
- Duplicate the Neon scene, generate new asset GUIDs, and bind every bootstrap TextAsset slot to one content tree. Do not copy the controller or weapon execution code.

## Runtime And Authored Boundaries

| Presentation area | Classification | Current boundary |
| --- | --- | --- |
| Player visual | Theme/config driven | Runtime capsule fallback uses authored `worldPresentation.playerColor`. |
| Normal, elite, miniboss, boss visual | Authored and replaceable now | Enemy role records own display names and tints; runtime primitives remain mesh fallbacks. |
| Projectile visual | Authored and replaceable now | Each launched projectile applies the active authored weapon tint; primitive sphere/trail remain fallbacks. |
| Weapon impact and telegraph geometry | Runtime primitive fallback | Behavior-colored or role-colored transient primitives; no alternate gameplay implementation. |
| XP, magnet, health, currency pickups | Theme/config driven | Semantic colors come from `worldPresentation`; primitive sphere meshes remain fallbacks. |
| Reward cache, boss/elite bars, threat markers | Theme/config driven | Elite/boss palette colors come from `worldPresentation`; role and health state remain runtime data. |
| Draft, weapon, passive, relic icons | Theme/config driven | Authored category/rarity icon IDs render through the current text-placeholder surface. A production texture resolver is not yet proven. |
| Arena/background | Theme/config driven | Scene ambient lighting plus authored floor, grid, accent, landmark, and compass colors; runtime tiled primitives remain fallback geometry. |
| UI, cards, banners, run summary | Theme/config driven | Theme text/tokens, rarity colors, HUD accent, authored content names, and authored reward labels. |
| Audio events | Authored and replaceable now | Event IDs, categories, volume, and throttling are authored. Generated tones remain the missing-clip fallback. |

Primitive meshes, text icon placeholders, and generated tones are explicit fallback presentation, not hidden Basic content. A future production-art pass may add typed prefab, texture, material, and AudioClip references. That is not extracted into a package until another game template proves a generic contract.

## Strictness And Tests

Both scenes use `SurvivorsAuthoredContentBindingPolicy.StrictSample`. Missing required fields, invalid upgrade/evolution/class/reward references, missing enemy roles, invalid profile tuning, missing theme/tutorial/audio tokens, and invalid authored world colors block run start. `SurvivorsEditorContentValidation.ValidateContent` validates both complete trees.

`SurvivorsAssetFlipEditModeTests` proves independent validation, strict/no-fallback binding, distinct names, one starter, populated drafts, authored rewards/progression, both pacing profiles, theme/tutorial/audio/world presentation, mutation behavior, independent scene GUIDs, and no alternate gameplay scripts. `SurvivorsAssetFlipPlayModeTests` boots the imported Neon scene and proves strict selection, Sprint, Standard, runtime names, player palette, authored projectile tint, draft population, and authored meta labels.

## Package Extraction Readiness

Ready for second-template evaluation, but not extraction by this pass:

- strict authored binding and cross-library validation
- reward draft and rarity presentation
- comparison previews and build summaries
- run-summary model/UI
- tutorial overlay and audio event routing
- theme/world-presentation tokens
- run-profile selection
- health bars, threat markers, and offscreen spawn resolution

Still needs Idle Auto Defense or another genuinely different game template before package extraction:

- generic content-pack manifests beyond serialized sample-scene bindings
- typed prefab/material/icon/audio references
- generic theme and audio schemas
- generic reward-card, run-summary, tutorial, marker, and validation helpers

Keep local to this template:

- every Survivors weapon behavior mapping, enemy role, boss, evolution, pickup rule, reward rule, run balance, content tree, sample scene, controller adapter, and debug command

No Game Content Authoring package change was needed. The existing JSON/TextAsset binding and validation adapter were sufficient; introducing a generic manifest or typed Unity-reference contract without a second game consumer would be premature.

## Manual Proof Checklist

1. Open Basic Survivors, start Sprint, and confirm the original names, one starter, draft, enemies, boss, rewards, and summary.
2. Open Neon Arcana, confirm the selector reports strict authored content, and start Sprint.
3. Confirm Neon Pulse, Pulse Runner, Static Wisp, Firewall Sentinel, Hexframe Prime, and Blacklight Sovereign appear at runtime.
4. Confirm the cyan/magenta world palette, alternate tutorial, alternate theme choices, and different audio-event palette.
5. Confirm exactly one starting weapon, later weapons from drafts, elites/bosses offscreen, upgrades/evolutions/rewards, and victory/defeat/restart.
6. Start Neon Arcana Standard and confirm the 30-minute target and endless continuation remain intact.
7. Confirm neither scene reports fallback content and no alternate controller or weapon implementation exists.
