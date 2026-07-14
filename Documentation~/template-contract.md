# Survivors Template Contract

This contract protects Template-Game-Survivors as a complete playable game template. It applies to package extraction, major refactors, asset-flip work, validation changes, and future reusable-system proposals.

## Guiding Rule

“Extract only reusable infrastructure, never the playable vertical slice.”

## Template Contract

- This template must always import/open as a complete playable vertical slice.
- Package extraction may only move reusable infrastructure out of the template.
- Package extraction must not remove the sample game, authored content, tuning, UI, themes, audio palette, tutorial copy, sample scene, or playable loop.
- After any extraction, the Basic Survivors sample must still play directly from the sample scene with no manual reconstruction.
- The template is not allowed to become a hollow framework.
- The sample must remain asset-flippable through authored content/config.
- The default user experience after import must be: open sample scene, press Play, choose a mode, play the game.
- Fallback/default runtime code may exist only as safe/debug/unbound-host fallback. It must not be the normal path for the Basic Survivors sample.
- Runtime code owns logic/execution. Authored content owns asset-flippable game content.
- Any future package extraction must prove the playable sample still works.

## Concrete Asset-Flip Proof

`Samples~/BasicSurvivorsGame/Scenes/NeonArcana.unity` and `Samples~/BasicSurvivorsGame/Content/NeonArcana` are the required concrete proof that this contract is real. The alternate scene uses the same `BasicSurvivorsGameBootstrap`, `SurvivorsTemplateController`, weapon executors, spawning, combat, XP, drafts, evolutions, rewards, profiles, persistence, and restart flow. It binds a separate complete authored tree, contains no alternate controller or genre runtime, and cannot reference or silently borrow a Basic `Default*` content asset.

Future changes must keep both Basic Survivors and Neon Arcana strict, directly playable, and able to start Standard and Sprint with exactly one authored starting attack. `Documentation~/neon-arcana-asset-flip.md` owns the replacement map, visual-boundary audit, and extraction-readiness classification.

## What Stays In The Template

- The Basic Survivors sample scene and bootstrap.
- The authored sample content under `Samples~/BasicSurvivorsGame/Content`.
- Survivors-specific weapons, projectiles, passives, evolutions, relics, enemies, elites, bosses, rewards, classes, progression tracks, run-flow profiles, tuning, and balance.
- The Standard / Human Playtest and Sprint Run mode definitions as authored sample content.
- The player-facing loop: run-mode selection, movement, auto-attacks, XP collection, drafts, pickup/magnet builds, elites/minibosses/bosses, rewards, victory/defeat, restart/change-mode, and run summary.
- Template-owned UI assembly for the current vertical slice: HUD, draft cards, build menu, mode selector, tutorial overlay, theme selector, result summary, life bars, threat markers, and debug overlay entry points.
- Theme, audio palette, tutorial, result, rarity, category, and button copy examples that prove asset-flip readiness.
- Template-specific validation for Survivors authored schemas and vertical-slice minimums.

## What May Move To Packages

Only reusable infrastructure may move out after at least one more concrete template or product proves the same need with different game content. Candidate areas include presentation primitives, reward-card view-model contracts, stat-preview formatting, audio event routing, theme tokens, generic validation helpers, and debug overlay scaffolding.

An extracted package must accept game-owned content and callbacks. It must not own Survivors weapons, enemies, bosses, evolutions, run balance, sample bootstrap, sample scene, or the playable loop.

## What Must Never Move Out Without Replacement

- The playable Basic Survivors sample scene.
- The authored content libraries that make the sample asset-flippable.
- Standard and Sprint run-flow profiles.
- The default run-mode selection experience.
- Draft UI and reward resolution sufficient to play the sample.
- Pickup/magnet build choices and their runtime effects.
- Elite, miniboss, boss, and final-boss content plus their life bars, markers, rewards, and restart/victory flow.
- Theme/audio/tutorial content that makes the sample feel like a product instead of a code-only framework.
- Validation proving authored content is the normal Basic sample path.

If a future extraction removes local implementation code, the replacement must be wired into the sample before the change is considered complete.

## Authored-Content Driven Boundary

The Basic Survivors sample must remain driven by editable authored content/config for asset-flippable areas:

- `Samples~/BasicSurvivorsGame/Content/DefaultWeapons/weapons.json`
- `Samples~/BasicSurvivorsGame/Content/DefaultUpgrades/upgrades.json`
- `Samples~/BasicSurvivorsGame/Content/DefaultEnemies/enemies.json`
- `Samples~/BasicSurvivorsGame/Content/DefaultRunFlow/run-flow.json`
- `Samples~/BasicSurvivorsGame/Content/DefaultRewards/rewards.json`
- `Samples~/BasicSurvivorsGame/Content/DefaultProgression/progression.json`
- `Samples~/BasicSurvivorsGame/Content/DefaultRelics/relics.json`
- `Samples~/BasicSurvivorsGame/Content/DefaultClasses/classes.json`
- `Samples~/BasicSurvivorsGame/Content/DefaultPickups/pickups.json`
- `Samples~/BasicSurvivorsGame/Content/DefaultUiTheme/ui-theme.json`
- `Samples~/BasicSurvivorsGame/Content/NeonArcanaUiTheme/ui-theme.json`

Runtime fallback content is allowed for tests, unbound hosts, and debug safety. It must not become the normal path for the Basic Survivors sample.

### Strict Basic Sample Policy

`BasicSurvivorsGameBootstrap` configures `SurvivorsAuthoredContentBindingPolicy.StrictSample` before it opens mode selection. The strict bind includes weapons, upgrades, relics, classes, progression, enemies, run flow, rewards, pickups, both sample themes, the audio palette, and tutorial copy. A missing or invalid required value leaves the controller in its pre-run state, disables Standard/Sprint start, and reports the validation error. It must never continue by silently launching `BasicSurvivorsGame` fallback balance.

`SurvivorsAuthoredContentBindingPolicy.AllowFallbacks` is reserved for the legacy partial binder, programmatic tests, runtime debugging, and hosts that intentionally do not provide the complete Basic sample. The controller exposes this state through `IsFallbackContentActive` and an authored-content status beginning with `Fallback content active`.

### Field Classification

| Classification | Basic sample policy |
| --- | --- |
| Required authored field | Weapon identity/display/archetype/tint/cooldown/damage/range and relevant archetype values; upgrade identity/display/category/rarity/weight/max rank/effects/targets/amounts; enemy identity/display/role/tint/combat/lifecycle/marker/life-bar values; relic/class/progression/reward references and amounts; required run-flow profiles, rarity tables, shared gameplay tuning, pickup manifest entries, theme tokens, audio IDs, and tutorial copy. Missing or invalid values fail strict validation. |
| Optional authored field with documented default | Omitted zero-count projectile mutations and false capability flags mean zero/false where validation allows that state. Empty prerequisite/reference fields mean no prerequisite/reference. `affectedContentId` and `requiredOwnedWeaponId` may be derived from the authored upgrade category/target. An omitted `gameplayTuningOverrides` field inherits the required authored `sharedGameplayTuning` value. These defaults never consult `BasicSurvivorsGame` in strict mode. |
| Runtime algorithmic value | Deterministic seeds/selection hashes, role weighting inside authored pools, elapsed-time phase evaluation, collision and damage execution, clamping, pooling, rendering, and the formula that scales an authored Human enemy baseline by authored profile basis values. These are implementation rules rather than game records. |
| Debug/unbound fallback only | `BasicSurvivorsGame` catalogs and tuning factories, built-in class/relic/progression/theme defaults, and the three-file legacy binder remain available only to explicit fallback hosts, tests, and debug recovery. They are not consulted to fill strict sample fields. |

`DefaultRunFlow/run-flow.json` uses one explicit authored inheritance rule. `sharedGameplayTuning` is a complete required baseline for player movement/health/dash, spawn geometry and crowd spacing, threat warnings/slams/support, pickup/reward feedback, waystones, statuses, barriers, and surge feedback. Each profile may provide non-zero `gameplayTuningOverrides`; omission means inherit from that authored baseline. Profile pacing fields remain fully explicit per profile. `PacingProfile` identity and deterministic `RunSeed` remain runtime algorithmic values.

### Progression And Reward Ownership

`DefaultProgression/progression.json` owns class passive atlases, weapon skill tracks, track nodes, evolution nodes, node tiers/costs/max ranks, and class-specific upgrade gates. Class-owned passive-atlas nodes actively drive strict runtime class gates; upgrade JSON does not duplicate `allowedClasses` ownership. Weapon-track grouping, tier, point-cost, and node-rank data is currently bound and validated reference/tooling metadata: the live run does not spend progression points, and upgrade/evolution rank requirements still come from `DefaultUpgrades/upgrades.json`. Mutation tests prove both the live class eligibility path and the bound weapon/evolution metadata change when progression JSON changes.

`DefaultRewards/rewards.json` owns currencies, run reward grants, elite/miniboss/boss/class-unlock reward amounts, legacy-XP grants, and the current persistent meta-upgrade definitions/costs/effects. It does not own class gates, weapon tracks, or evolution-node structure.

Game Content Authoring owns the reusable edit-session lifecycle, source locking, canonical record-reference selector, ordered/structured workbench, same-pack candidate orchestration, stale/conflict presentation, and commit/rollback orchestration. Survivors owns its strict UTF-8 token/row-span locator, approved field and effect-row schemas, canonical-key-to-JSON-ID mapping, closed gameplay token sets, Passive prerequisite compatibility, path policy, proposed-pack validation adapter, atomic JSON backend, and recovery bytes. An evolution's required Passive prerequisite is the only editable canonical reference. An Upgrade's direct `Effects` rows are editable only as anonymous embedded values with `effect`, `target`, and `amount`; they never become canonical records or top-level CRUD. Every stable ID and other reference remains read-only. JSON remains the sole runtime source of truth; safe editing never creates a ScriptableObject mirror or moves playable content into the generic authoring package.

## Vertical-Slice Preservation Checklist

After any package extraction or major refactor, verify:

- Basic Survivors sample scene opens.
- Mode selector appears.
- Sprint Run starts.
- Standard / Human Playtest starts.
- Top-center timer works.
- Player starts with authored starting loadout.
- Weapons come from authored content.
- Upgrades/passives/evolutions come from authored content.
- Enemies/elites/bosses come from authored content.
- Sprint/Standard profiles come from authored run-flow content.
- Rewards/progression/theme/audio/tutorial content are authored or explicitly documented.
- Enemies spawn offscreen in normal gameplay.
- Draft UI works.
- Pickup/magnet build choices work.
- Elite/boss markers/life bars work.
- Victory/defeat/restart flow works.
- Run summary works if present.
- Debug UI is hidden by default.
- Tests pass.
- No manual reconstruction is required after import.

## Required Validation After Extraction

Run the normal package validation plus enough Unity coverage to prove the sample still plays:

- `git diff --check`
- Deucarian package validator
- editor content validation or equivalent content-validation test coverage
- EditMode tests
- PlayMode tests when practical
- restricted text scan when part of the local workflow

Manual validation should open the imported Basic Survivors playtest scene, press Play, choose Standard, choose Sprint after restart/change-mode, resolve at least one draft, observe the top-center timer, confirm authored HUD/build content, and confirm victory/defeat/restart still works.

## Existing Guardrails

- `Documentation~/survivors-template-structure.md` maps runtime, authored sample content, and test coverage.
- `Documentation~/playtesting.md` is the manual playtest checklist for Standard, Sprint, UI, rewards, tutorial, audio, and small-screen checks.
- `Documentation~/validation.md` records validation expectations and current coverage phases.
- `Documentation~/survivors-package-extraction-candidates.md` lists candidate package boundaries and their vertical-slice preservation risk.
- EditMode and PlayMode tests cover authored content binding, vertical-slice shape, mode selection, Sprint/Standard profile separation, UI/HUD flows, theme/audio/tutorial loading, and sample scene boot.

## Idle Auto Defense Reuse

Idle Auto Defense can be used as the second-template proof for reusable infrastructure, but only by proving the same infrastructure works with different content, pacing, UI copy, and game rules. Its existence does not justify removing Survivors content from this template. Any shared package must serve both games while each template keeps its own playable vertical slice.

## Known Contract Risks / Future Hardening

- Keep the strict schema and authored tuning-parity mutation tests current when adding new gameplay fields.
- Keep the generic ordered-collection workbench in Game Content Authoring while tutorial copy, the 1-3 row layout limit, JSON mapping, validation, and runtime display remain owned by this template.
- Keep the generic structured-row workbench in Game Content Authoring while the Survivors Upgrade Effects schema, closed effect/target tokens, lossless JSON mapping, strict validation, and gameplay consumption remain owned by this template. Do not turn embedded rows into canonical records to avoid implementing CRUD.
- Do not let copied/imported sample scenes become stale when bootstrap content references change.
- Keep offscreen spawn resolver package candidacy separate from Survivors enemy content, tuning, and reward rules.
- Prove extraction with Idle Auto Defense before moving shared systems broadly.

These are guardrail concerns and future hardening tasks. They are not reasons to hollow out the template.
