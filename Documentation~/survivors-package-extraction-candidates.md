# Survivors Package Extraction Candidates

This polish pass keeps every system local to Template-Game-Survivors. No package extraction, publishing, registry update, or shared dependency change was performed.

Use these notes after a second game/template proves the same needs with different content.

## Reward Draft UI / Card System

- Reusable because many templates need 1-of-N choices with rarity, category, affected item, hotkeys, reroll, banish, skip, and touch-sized card controls.
- Survivors-specific dependencies: `RunUpgradeDefinition`, Survivors upgrade metadata, relic drafts, level-up/reward selection state, and local theme tokens.
- Should remain local for now: exact card text, evolution hints, Sprint draft pacing, boss relic chaining, and local IMGUI implementation.
- Second proof needed: another template using a different reward source and different categories while preserving hotkeys and reroll/banish/skip semantics.
- Likely boundary: a reward-choice presentation package that accepts plain card view models plus command callbacks.

## Upgrade Comparison Preview Helper

- Reusable because draft choices read better when numeric effects show before-to-after values.
- Survivors-specific dependencies: Survivors effect IDs, runtime stat properties, weapon/passive/evolution metadata, and pickup/magnet terminology.
- Should remain local for now: exact stat mapping and fallback strings for Survivors weapons, passives, evolutions, and collector effects.
- Second proof needed: another upgrade-driven template using different stat names and effect IDs.
- Likely boundary: a stat-preview formatter with registered stat readers and effect descriptor adapters.

## Run Summary UI

- Reusable because arcade templates usually need a result screen with mode, result, time, kills, rewards, build, restart, and change-mode actions.
- Survivors-specific dependencies: Survivors run state, meta rewards, class selection, persistent upgrade purchases, relics, evolutions, and pickup/magnet stats.
- Should remain local for now: class unlock/purchase options, Endless Continue, boss/miniboss terminology, and current summary line model.
- Second proof needed: another template with a different end-state model and reward economy.
- Likely boundary: a result-summary view-model and action surface package, not the Survivors reward calculation.

## Tutorial / Onboarding Overlay

- Reusable because first-run tutorials need seen-state persistence, skip/next/back controls, and pause integration.
- Survivors-specific dependencies: tutorial copy, movement/combat/draft/evolution/mode concepts, and the local meta profile flag.
- Should remain local for now: exact seven-step script and run-start timing.
- Second proof needed: another template with a different tutorial flow and persistence owner.
- Likely boundary: a lightweight onboarding overlay package that consumes card text, actions, and a persistence adapter.

## Audio Event Palette / Event Routing

- Reusable because product templates need event IDs, categories, volume, throttling, mute, and missing-clip-safe dispatch.
- Survivors-specific dependencies: event names for drafts, elites, bosses, pickup/magnet, evolutions, and the local placeholder audio clips.
- Should remain local for now: event ID taxonomy and direct controller dispatch points.
- Second proof needed: another template using the same palette idea with different event IDs and real audio assets.
- Likely boundary: an audio-event router plus serializable palette tokens.

## Mobile-Safe UI Primitives

- Reusable because IMGUI overlays need safe centered panels, fixed action buttons, and scrollable content on small game views.
- Survivors-specific dependencies: current IMGUI styles and panel placement for selector, drafts, tutorial, build menu, and summary.
- Should remain local for now: concrete panel sizes, card layout, and HUD overlap behavior.
- Second proof needed: another IMGUI or runtime UI template with different panel content.
- Likely boundary: small safe-rect/scroll helpers or a UI Toolkit replacement package after more proof.

## Theme / Style Token System

- Reusable because asset flips need configurable labels, rarity accents, categories, button copy, HUD accent, icon placeholders, and audio palette tokens.
- Survivors-specific dependencies: rarity/category IDs, mode names, draft labels, and Neon Arcana sample content.
- Should remain local for now: exact token vocabulary and JSON schema.
- Second proof needed: another game template that can switch visual style from config without gameplay changes.
- Likely boundary: a small theme-token package plus schema validation.

## Build Menu / Stat Summary Model

- Reusable because player-facing build/status menus are common in progression-heavy templates.
- Survivors-specific dependencies: weapons, passives, relics, evolutions, pickup/magnet stats, run info, and controls.
- Should remain local for now: tab names, stat groupings, and rank formatting.
- Second proof needed: another template that needs similar build introspection but not Survivors terminology.
- Likely boundary: a stat-summary view-model contract with game-owned stat providers.

## Rarity Presentation Helpers

- Reusable because rarity names, accent colors, frame tokens, and icons are not unique to Survivors.
- Survivors-specific dependencies: RunUpgrade rarity enum, Evolution and Relic pseudo-rarity IDs, and card drawing.
- Should remain local for now: exact colors, fallback IDs, and card accent behavior.
- Second proof needed: another template with rarity-like tiers and a different card surface.
- Likely boundary: shared rarity token schema plus lookup helpers.

## Content Validation Helpers

- Reusable because authored content needs duplicate ID checks, reference checks, positive-value checks, and player-facing text validation.
- Survivors-specific dependencies: weapon/relic/class/run-flow schemas, enemy lifecycle flags, major reward IDs, pickup/magnet coverage, and Sprint pacing constraints.
- Should remain local for now: all Survivors schema rules and pacing assertions.
- Second proof needed: another authored template with overlapping validation shapes but different schemas.
- Likely boundary: generic validation utilities only, while each template owns its schema-specific rules.
