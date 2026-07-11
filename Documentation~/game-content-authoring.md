# Survivors Game Content Authoring

## Entry Point

Import the `Basic Survivors Game` sample, then open:

`Tools > Deucarian > Game Content Authoring`

Select `Survivors Content Packs`. Exactly one Survivors provider exposes two imported manifests:

- `Basic Survivors`, backed by the `Default*` JSON tree and `BasicSurvivorsGame.unity`.
- `Neon Arcana`, backed by `Content/NeonArcana` and `NeonArcana.unity`.

The manifests live under `Samples~/BasicSurvivorsGame/ContentPacks`. They contain generic pack metadata plus Unity references to scenes and `TextAsset` sources. They do not contain copied gameplay records.

## Browser Model

The Survivors provider parses each selected manifest source into editor-only projections. JSON remains the source of truth used by gameplay. Records have pack-scoped IDs, source locators, player-facing metadata, validation, and inbound/outbound references.

The browser exposes Weapons, Projectiles, Upgrades, Passives, Pickup / Magnet, Mutations, Evolutions, Relics, Classes, Enemies, Elites, Minibosses, Bosses, Run Profiles, Waves / Milestones, Rewards, Meta Upgrades, Progression, Themes, Audio Events, and Tutorial.

Semantic categories are filtered views, not duplicate JSON records. For example, an elite remains one enemy identity, a pickup passive remains one upgrade identity, and a run profile remains one identity in both Run Profiles and Waves / Milestones. Basic references resolve only inside Basic; Neon references resolve only inside Neon.

## Validation And Actions

`Validate` passes the selected manifest's actual weapon, upgrade, enemy, reward, relic, class, progression, pickup, run-flow, primary-theme, and alternate-theme `TextAsset` contents to `SurvivorsContentValidator` through the existing Game Content Authoring validation adapter. It does not validate package-default file paths behind the manifest.

Available pack actions are:

- `Open Scene`: opens the selected imported scene.
- `Play Standard`: enters Play Mode and calls the existing strict Standard / Human Playtest selection, targeting 1800 seconds.
- `Play Sprint`: enters Play Mode and calls the existing strict Sprint selection, targeting 300 seconds.
- `Validate`: validates selected sources independently.
- `Browse Content`: focuses the shared Game Content Authoring window.
- `Reveal Source`: selects the imported manifest.
- `Open Package Installer`: opens the owner of sample installation and freshness.

The launch bridge is editor-only. It does not add a gameplay controller, rename a profile, weaken strict authored startup, or alter runtime tuning.

## Missing Or Duplicate Samples

When the sample is not imported, the provider still registers and shows `Survivors Sample Not Imported`. Scene and play actions are disabled, and `Open Package Installer` explains how to import the sample. Game Content Authoring does not copy, refresh, or synchronize samples.

Multiple imported manifests with the same `(owningPackageId, packId)` are blocking conflicts. Every conflicting source location remains inspectable; no copy is selected silently.

## Read-Only Scope

Milestone 1 supports discovery, browsing, search, filtering, sorting, metadata, references, validation, reveal, and launch actions. JSON editing, write-back, Undo, record CRUD, pack cloning, sample freshness/sync, typed visual asset authoring, and gameplay changes are deferred.

This follows the Template Contract: "Extract only reusable infrastructure, never the playable vertical slice."
