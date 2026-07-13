# Survivors Game Content Authoring

## Entry Point

Import the `Basic Survivors Game` sample, then open:

`Tools > Deucarian > Game Content Authoring`

Use the global `Content Pack` selector to choose one of two imported manifests:

- `Basic Survivors`, backed by the `Default*` JSON tree and `BasicSurvivorsGame.unity`.
- `Neon Arcana`, backed by `Content/NeonArcana` and `NeonArcana.unity`.

The manifests live under `Samples~/BasicSurvivorsGame/ContentPacks`. They contain generic pack metadata plus Unity references to scenes and `TextAsset` sources. They do not contain copied gameplay records.

## Browser Model

The Survivors provider parses each selected manifest source into editor-only records. JSON remains the source of truth used by gameplay. Records have canonical `(owningPackageId, packId, sourceId, sourceRecordId)` keys, source locators, typed capabilities, player-facing metadata, validation, and inbound/outbound references.

The browser exposes Weapons, Projectiles, Upgrades, Passives, Pickup / Magnet, Mutations, Evolutions, Relics, Classes, Enemies, Elites, Minibosses, Bosses, Run Profiles, Waves / Milestones, Rewards, Meta Upgrades, Progression, Themes, Audio Events, and Tutorial.

Semantic categories and domain lenses are filtered views, not duplicate JSON records. For example, an elite remains one enemy identity, a pickup passive remains one upgrade identity, and a run profile remains one identity in both Run Profiles and Waves / Milestones. Basic references resolve only inside Basic; Neon references resolve only inside Neon.

## Reusable Lens Mappings

- Each of the 10 weapon records exposes both Weapon and Attack capabilities. The same key and record instance drives the Attacks and Weapon / Tower lenses; Survivors does not create a second attack catalog.
- The projectile record exposes Projectile. Weapon/Attack references navigate to that canonical record.
- All 10 enemy records expose Enemy. Elite, Miniboss, Boss, and Major Threat capabilities add semantic views to the same records and expose authored combat, leash, marker, and life-bar values.
- The five run-flow profiles expose Encounter, Run Profile, Timed Milestone, Horde Event, Elite Event, and Boss Event capabilities. The timeline projection reads Standard at 1800 seconds and Sprint at 300 seconds plus authored escalation and enemy/reward references.
- Authored run upgrades and meta upgrades expose Upgrade plus Weapon Upgrade, Passive, Pickup / Magnet, Mutation, Evolution, or Meta Upgrade capabilities where applicable. The Upgrades lens filters those capabilities without changing identity.
- All Content shows the complete selected 251-record pack, or both packs in read-only All Packs mode, with search, capability/source/validation filters, sorting, references, and compatible-lens navigation.

The domain packages own their common projection types and preview UI. This template owns the adapters and Survivors-specific metadata. Game Content Authoring orchestrates selection and identity but does not parse Survivors JSON.

## Validation And Actions

`Validate` passes the selected manifest's actual weapon, upgrade, enemy, reward, relic, class, progression, pickup, run-flow, primary-theme, and alternate-theme `TextAsset` contents to `SurvivorsContentValidator` through the existing Game Content Authoring validation adapter. It does not validate package-default file paths behind the manifest.

Available Pack Dashboard actions are:

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

## Existing-Record Editing

Basic Survivors and Neon Arcana enable `Edit Existing` only for project-owned imported copies under `Assets/Samples`. Package `Samples~`, `Packages`, `PackageCache`, `Library`, `Temp`, paths outside `Assets`, path traversal, reparse-point escapes, missing files, and read-only files are rejected. All Packs, Project Content, conflicted manifests, invalid packs, unsupported records, and platforms/filesystems without verified atomic replacement remain read-only with an actionable reason.

The first edit session in an editor session warns that this is an imported copy. A future package sample update or reimport may replace or conflict with local edits; Package Installer owns sample refresh and synchronization. Game Content Authoring does not synchronize samples.

Supported direct properties are determined from each actual record object:

| Record | Editable direct scalar fields | Visible read-only fields |
|---|---|---|
| Weapon | `displayName`, `cooldownSeconds`, `damage`, and direct `range`, `radius`, or `projectileRadius` where present | `id`, `fireMode`, direct `projectileId` where present |
| Projectile | `speed`, `lifetimeSeconds` | `id` |
| Enemy | `displayName`, `health`, `contactDamage`, `contactIntervalSeconds`, `moveSpeed`, `radius`, `experienceDrop` | `id`, `role` |

Upgrades, passives, mutations, evolutions, relics, classes, run flow, rewards, progression, pickups, themes, audio, tutorial, lists, references, asset fields, stable IDs, create, duplicate, delete, bulk edits, multi-file transactions, cross-pack copy/move, and pack cloning remain deferred.

## Lossless Save And Recovery

The template owns an editor-only strict UTF-8 tokenizer and ID-based record locator. It preserves BOM state, property/record order, unknown fields, whitespace, mixed line endings, and every unrelated scalar token. It does not use `JsonUtility` or another DTO serializer for full-file write-back. One source session targets one canonical record; another record in the same physical JSON file reports a source-lock conflict, while the same canonical record reached through another lens attaches to the existing session.

Apply, Undo, Redo, and Preview are in-memory. Preview rejects stale bytes/path/GUID/manifest-source-list revisions and validates the proposed complete pack by substituting the proposed source text into the existing strict Survivors validator without writing it. Errors block save and warnings require explicit confirmation. Cancel before commit restores nothing because it changes exactly zero source bytes.

Commit writes a flushed temporary file beside the destination and uses platform `File.Replace`; it never deletes the destination first. Exact original bytes and transaction metadata are prepared under `Library/Deucarian/GameContentAuthoring/Recovery/Survivors` before replacement. The backend then synchronously reimports, verifies the exact new hash and edited tokens, reruns strict pack validation, and reindexes the selected pack. Basic and Neon use different GUIDs, paths, lock keys, and pack-scoped record keys even where logical record IDs match.

Rollback after a successful commit first requires the current source to match the committed revision, then atomically restores and verifies the exact previous bytes. It refuses to overwrite a later external edit. Up to five resolved backups per physical source are retained; unresolved recovery records are retained for explicit review. In-session Undo/Redo does not promise cross-session Unity Undo.

Existing standalone ScriptableObject authoring remains separate under the synthetic Project Content pack. JSON remains the only gameplay source of truth and no ScriptableObject mirror is created.

This follows the Template Contract: "Extract only reusable infrastructure, never the playable vertical slice."
