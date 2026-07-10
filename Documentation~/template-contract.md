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

- Some authored binders may use built-in per-field fallback values when optional fields are missing.
- `DefaultProgression/progression.json` is parsed and validated, but its exact live gameplay role should be clearly documented or hardened in a separate pass.
- Remove or reduce hidden scalar/effect fallbacks for asset-flip-critical fields.
- Make `DefaultProgression/progression.json`'s exact runtime role explicit in docs and validation.
- Keep offscreen spawn resolver package candidacy separate from Survivors enemy content, tuning, and reward rules.
- Prove extraction with Idle Auto Defense before moving shared systems broadly.

These are guardrail concerns and future hardening tasks. They are not reasons to hollow out the template.
