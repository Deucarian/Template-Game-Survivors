# Survivors Package Extraction Candidates

This polish pass keeps every system local to Template-Game-Survivors. No package extraction, publishing, registry update, or shared dependency change was performed.

Neon Arcana now proves a second complete content variation inside this template can use the same strict binding, runtime, UI, and presentation hooks. That is sufficient for asset-flip confidence and for evaluating the candidates below. It is not a second game-template proof: Idle Auto Defense or another distinct game must still consume a genuinely generic boundary before anything moves to a shared package.

Use these notes only with `Documentation~/template-contract.md`. The governing rule is: “Extract only reusable infrastructure, never the playable vertical slice.”

Before any extraction, protect these local boundaries:

- Specific Survivors weapons/enemies/bosses/evolutions remain local content.
- Survivors tuning and balance remain local content.
- The Basic Survivors sample scene remains in the template.
- The playable vertical slice remains in the template.
- Extraction must be tested by the template still playing.
- Fallback/default runtime content must stay safety/debug support, not the normal Basic Survivors sample path.

Use these candidates after a second game/template proves the same needs with different content. If extraction cannot preserve the sample scene, authored content, run-mode selection, HUD, draft/reward loop, and restart/victory flow, do not extract it.

## Reward Draft UI / Card System

- Why reusable: many templates need 1-of-N choices with rarity, category, affected item, hotkeys, reroll, banish, skip, and touch-sized card controls.
- Survivors-specific: `RunUpgradeDefinition`, Survivors upgrade metadata, relic drafts, level-up/reward selection state, Sprint draft pacing, boss reward chaining, and local theme tokens.
- What stays local: exact Survivors card text, evolution hints, reward-tier rules, boss relic chaining, pickup/magnet terminology, and the current IMGUI implementation until a shared UI surface is proven.
- Second template proof: another template using a different reward source and different categories while preserving command semantics.
- Likely package boundary: a reward-choice presentation package that accepts plain card view models plus command callbacks.
- Vertical-slice preservation risk: extracting the card surface could leave the sample unable to resolve level-ups, elite rewards, boss evolution rewards, relic rewards, skip/reroll/banish, or result-screen restart commands.

## Rarity Tables And Rarity Presentation

- Why reusable: rarity weights, labels, accents, frames, and icon tokens are common to card and reward surfaces.
- Survivors-specific: normal/elite/boss rarity tables, RunUpgrade rarity enum use, Evolution and Relic pseudo-rarity IDs, Sprint/Standard rarity pacing, and card drawing.
- What stays local: Survivors rarity tuning, boss reward weighting, fallback reward rules, exact colors, fallback IDs, and card accent behavior.
- Second template proof: another template with rarity-like tiers, different pacing rules, and a different reward surface.
- Likely package boundary: shared rarity token schema plus lookup/formatting helpers, not gameplay reward tables.
- Vertical-slice preservation risk: moving rarity tables instead of presentation helpers could demolish Sprint/Standard reward pacing or strip authored rarity data from the sample.

## Upgrade Comparison Preview Helper

- Why reusable: draft choices read better when numeric effects show before-to-after values.
- Survivors-specific: Survivors effect IDs, runtime stat properties, weapon/passive/evolution metadata, and pickup/magnet terminology.
- What stays local: exact stat mapping and fallback strings for Survivors weapons, passives, evolutions, collector effects, and authored upgrade semantics.
- Second template proof: another upgrade-driven template using different stat names, effect IDs, and build state.
- Likely package boundary: a stat-preview formatter with registered stat readers and effect descriptor adapters.
- Vertical-slice preservation risk: extracting effect interpretation could hide build changes, expose internal IDs, or break pickup/magnet previews that teach the asset-flip build path.

## Run Summary UI / Model

- Why reusable: arcade templates usually need a result screen with mode, result, time, kills, rewards, build, restart, and change-mode actions.
- Survivors-specific: Survivors run state, meta rewards, class selection, persistent upgrade purchases, relics, evolutions, Endless Continue, and pickup/magnet stats.
- What stays local: class unlock/purchase options, Endless Continue, boss/miniboss terminology, Survivors reward calculation, and current summary line content.
- Second template proof: another template with a different end-state model, reward economy, and restart/change-mode flow.
- Likely package boundary: a result-summary view-model and action surface package, not Survivors reward or meta calculations.
- Vertical-slice preservation risk: extracting the summary flow could break victory/defeat/restart, class unlocks, meta purchases, or Sprint's no-endless result behavior.

## Tutorial / Onboarding Overlay

- Why reusable: first-run tutorials need seen-state persistence, skip/next/back controls, touch-friendly buttons, and pause integration.
- Survivors-specific: tutorial copy, movement/combat/draft/evolution/pickup/magnet/mode concepts, and the local meta profile flag.
- What stays local: exact onboarding script, timing after mode selection, Standard/Sprint descriptions, and Survivors profile persistence mapping.
- Second template proof: another template with a different tutorial flow, different control scheme, and different persistence owner.
- Likely package boundary: a lightweight onboarding overlay package that consumes authored card text, actions, and a persistence adapter.
- Vertical-slice preservation risk: extracting tutorial timing or persistence could block the first run, repeat onboarding every run, or remove player-facing mode/build explanations.

## Audio Event Palette / Event Routing

- Why reusable: product templates need event IDs, categories, volume, throttling, mute, and missing-clip-safe dispatch.
- Survivors-specific: event names for drafts, elites, bosses, pickup/magnet, evolutions, run summary, and local placeholder audio hooks.
- What stays local: event ID taxonomy, authored palette examples, controller dispatch points, and spam-throttle choices tied to Survivors pacing.
- Second template proof: another template using the same palette idea with different event IDs and real audio assets.
- Likely package boundary: an audio-event router plus serializable palette tokens.
- Vertical-slice preservation risk: extracting routing too early could silence important feedback for XP, level-up, warnings, rewards, victory, defeat, and summary.

## Mobile-Safe UI Primitives

- Why reusable: runtime overlays need safe centered panels, fixed action buttons, and scrollable content on small game views.
- Survivors-specific: current IMGUI styles and panel placement for selector, drafts, tutorial, build menu, HUD warnings, boss bars, and summary.
- What stays local: concrete panel sizes, card layout, HUD overlap behavior, top-center timer spacing, and theme-specific styling.
- Second template proof: another runtime UI template with different panel content and screen constraints.
- Likely package boundary: small safe-rect/scroll helpers or a UI Toolkit replacement package after more proof.
- Vertical-slice preservation risk: moving layout primitives without replacement could make the selector, drafts, tutorial, build menu, timer, or result screen unreadable on small screens.

## Theme / Style Token System

- Why reusable: asset flips need configurable labels, rarity accents, categories, button copy, HUD accent, icon placeholders, and audio palette tokens.
- Survivors-specific: rarity/category IDs, mode names, draft labels, tutorial/result copy, audio event keys, and Neon Arcana sample content.
- What stays local: exact token vocabulary, sample JSON files, theme selection behavior, and any tokens tied to Survivors gameplay concepts.
- Second template proof: another game template that can switch visual style from config without gameplay changes.
- Likely package boundary: a small theme-token package plus schema validation.
- Vertical-slice preservation risk: extracting style tokens as product content could remove Default/Neon theme authoring or make the sample depend on hidden hardcoded copy.

## Build Menu / Stat Summary Model

- Why reusable: player-facing build/status menus are common in progression-heavy templates.
- Survivors-specific: weapons, passives, relics, evolutions, pickup/magnet stats, run info, controls, class gates, and current rank formatting.
- What stays local: tab names, stat groupings, rank/evolution language, pickup/magnet presentation, and control copy.
- Second template proof: another template that needs similar build introspection but not Survivors terminology.
- Likely package boundary: a stat-summary view-model contract with game-owned stat providers.
- Vertical-slice preservation risk: extracting the model could hide current build state, break pickup/magnet stat display, or make the player unable to understand authored loadouts.

## Run Profile Selection

- Why reusable: templates often need a pre-run choice between profiles, descriptions, shortcuts, theme/settings hooks, and restart/change-mode behavior.
- Survivors-specific: Standard / Human Playtest, Sprint Run, Debug Fast separation, 30-minute and 5-minute copy, profile enum mapping, and tutorial/theme integration.
- What stays local: Survivors profile names, descriptions, timings, authored run-flow values, keyboard shortcuts, and debugger-only validation profiles.
- Second template proof: another template with multiple run modes backed by authored profiles and different gameplay semantics.
- Likely package boundary: a mode-selection shell that binds game-owned profile view models to callbacks.
- Vertical-slice preservation risk: extracting mode selection incorrectly could make the sample boot directly into combat again or make Sprint start Standard under a renamed label.

## Health Bars / Threat Markers

- Why reusable: many action templates need major-threat life bars, overhead bars, edge markers, distance labels, and warning presentation.
- Survivors-specific: elite, dread elite, miniboss, boss, final boss, reward guardian meaning, authored enemy lifecycle flags, support call-ins, slam telegraphs, and reward-state preservation.
- What stays local: enemy role names, marker priorities, boss/miniboss reward rules, life-bar thresholds, and major-threat tuning.
- Second template proof: another template with persistent important threats that need markers but different reward and enemy roles.
- Likely package boundary: a threat-indicator presentation layer fed by game-owned threat state.
- Vertical-slice preservation risk: extracting markers without preserving state could lose bosses/elites offscreen, hide reward guardians, or drop boss health/reward progress.

## Offscreen Spawn Resolver

- Why reusable: action games often need offscreen spawn/reposition bands that preserve pressure without instant contact.
- Survivors-specific: horde recycling, persistent major-threat catch-up, infinite arena recentering, run-flow spawn tuning, enemy roles, reward caches, and boss/elite health preservation.
- What stays local: Survivors spawn distances, normal-enemy recycle rules, major-threat reward rules, authored enemy lifecycle values, and arena trial/horde-rush spawning.
- Second template proof: another game needing offscreen pressure with different camera size, movement speed, enemy types, and reward rules.
- Likely package boundary: spawn-position/reposition helpers that accept game-owned constraints and threat-state callbacks.
- Vertical-slice preservation risk: extracting the resolver as gameplay logic could flatten Standard/Sprint pressure, spawn enemies onscreen, or lose persistent major threats and rewards.

## Content Validation Helpers

- Why reusable: authored content needs duplicate ID checks, reference checks, positive-value checks, and player-facing text validation.
- Survivors-specific: weapon/relic/class/run-flow schemas, enemy lifecycle flags, major reward IDs, pickup/magnet coverage, Sprint pacing constraints, and vertical-slice minimums.
- What stays local: all Survivors schema rules, authored content requirements, pacing assertions, and Basic sample vertical-slice checks.
- Second template proof: another authored template with overlapping validation shapes but different schemas.
- Likely package boundary: generic validation utilities only, while each template owns its schema-specific rules.
- Vertical-slice preservation risk: extracting schema rules could weaken authored-content guarantees or let fallback defaults become the normal Basic sample path.

## Runtime Debug Overlay Framework

- Why reusable: templates benefit from runtime debug panels for spawn metrics, profile switching, grants, state inspection, and reset hooks.
- Survivors-specific: XP grants, blood shards, horde rushes, elite/miniboss/boss forcing, Sprint boss forcing, magnet recall, build/evolution/draft inspection, and Survivors meta reset.
- What stays local: every debug action that mutates Survivors run state, grants Survivors rewards, forces Survivors enemies, or inspects Survivors-specific build data.
- Second template proof: another template with a similar debug shell but different commands and state models.
- Likely package boundary: a debug overlay host/command registry with game-owned command handlers.
- Vertical-slice preservation risk: extracting debug UI carelessly could expose debug controls by default, bypass authored mode selection, or reset/pollute sample save state.
