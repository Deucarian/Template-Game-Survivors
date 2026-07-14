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

Basic Survivors and Neon Arcana enable `Edit Existing` only for project-owned imported copies under `Assets/Samples`. Package `Samples~`, `Packages`, `PackageCache`, `Library`, `Temp`, paths outside `Assets`, path traversal, reparse-point escapes, missing files, and read-only files are rejected. All Packs, Project Content, conflicted manifests, unsupported records, and platforms/filesystems without verified atomic replacement remain read-only with an actionable reason. A validation-failed imported pack exposes only an existing tutorial step's `Lines` collection so blank or empty tutorial copy can be repaired; every other record remains read-only until the complete pack is valid.

The first edit session in an editor session warns that this is an imported copy. A future package sample update or reimport may replace or conflict with local edits; Package Installer owns sample refresh and synchronization. Game Content Authoring does not synchronize samples.

Supported direct properties are determined from each actual record object:

| Record | Editable direct fields | Visible read-only fields |
|---|---|---|
| Weapon | `displayName`, `cooldownSeconds`, `damage`, and direct `range`, `radius`, or `projectileRadius` where present | `id`, `fireMode`, direct `projectileId` where present |
| Projectile | `speed`, `lifetimeSeconds` | `id` |
| Enemy | `displayName`, `health`, `contactDamage`, `contactIntervalSeconds`, `moveSpeed`, `radius`, `experienceDrop` | `id`, `role` |
| Upgrade with a unique direct `effects` array | ordered structured `effects` rows | `id` and every other Upgrade property |
| Evolution with a unique direct `effects` array | required `requiredPassiveUpgradeId` canonical record reference and ordered structured `effects` rows | `id` and every other Evolution property |
| Tutorial step | ordered string collection `lines` | `id` |

The evolution prerequisite selector is supplied by the shared Game Content Authoring workbench. It derives candidates from the current selected pack, requires both Upgrade and Passive capabilities, and asks the Survivors session to approve the canonical target. Basic cannot select Neon, Neon cannot select Basic, the required field cannot be cleared, and no raw ID text box is exposed. The provider converts the approved canonical key to the target's existing JSON record ID only when building the proposed transaction.

For an Upgrade record, `Effects` is an `OrderedStructuredCollection` backed only by the selected pack's unique direct `upgrades[*].effects` array. An effect row is a parent-owned embedded value: it has a session-only row key and original index, but no authored ID, canonical GCA key, inbound references, save/progression identity, source record, or independent CRUD lifecycle. The parent Upgrade remains the sole canonical record before and after Add or Remove. Upgrades without a direct authored `effects` array remain read-only instead of synthesizing one from legacy scalar fields.

The row schema is explicit and provider-owned:

| Row field | Workbench control | Validation |
|---|---|---|
| `effect` | closed enum | one of the supported Survivors runtime/authored effect protocol tokens; arbitrary IDs are rejected |
| `target` | closed enum | one of the existing known Survivors gameplay target tokens; this mixed weapon/semantic protocol is neither free-form text nor a canonical-record reference |
| `amount` | number | invariant, finite, and non-zero |

Authored/runtime behavior accepts zero direct rows because the existing Upgrade object retains its authoritative legacy scalar fallback, so the descriptor does not invent a non-zero minimum. No runtime or validation rule establishes a maximum. Duplicate rows are allowed and remain independently addressable by session key; runtime consumes rows in authored array order. Add opens the generic complete-row form and stages nothing until the author explicitly supplies all three required values. Remove, Move Up/Down, Replace Row Field, Restore Original Order, Undo, and Redo operate only on staged values. Adding a row does not generate a persistent ID, and removing one does not delete the parent Upgrade.

For a Tutorial record, `Lines` is an `OrderedScalarCollection<string>` backed by the selected theme's direct `tutorialSteps[*].lines` array. The authored step ID provides canonical identity and remains visible but immutable; array index and display text are never identity. The current fixed-height, non-scrolling small-screen tutorial panel safely renders one to three rows, so the descriptor and runtime validation both enforce minimum `1` and maximum `3`. Blank or whitespace-only items are rejected, duplicate strings remain independently addressable through session keys, and runtime displays the committed sequence in exact collection order. Adding or removing a line never creates or deletes a tutorial step.

Other upgrade fields, Upgrade records without a direct effects array, relic/reward/progression/wave/class/theme/audio structured rows, nested or polymorphic effect rows, non-prerequisite evolution references, classes, run flow, rewards, progression, pickups, other tutorial fields, all other lists and references, asset fields, stable IDs, create, duplicate, delete, bulk edits, multi-file transactions, cross-pack copy/move, and pack cloning remain deferred.

## Lossless Save And Recovery

The template owns an editor-only strict UTF-8 tokenizer and ID-based record locator. It preserves BOM state, property/record order, unknown fields, whitespace, mixed line endings, and every unrelated token. It does not use `JsonUtility` or another DTO serializer for full-file write-back. Tutorial editing locates the selected theme source, direct root `tutorialSteps` array, one object by stable direct string `id`, and one unique direct `lines` string array. Effects editing locates the selected upgrades source, direct root `upgrades` array, one parent by stable direct string `id`, its unique direct `effects` array, every direct row object, and the exact direct scalar token for each mapped field. Duplicate parent IDs/properties, missing/non-array effects, non-object rows, duplicate child properties, wrong scalar types, nested unsupported properties, malformed JSON, nested-only matches, and ambiguous structures fail closed with an actionable reason. Unknown direct scalar properties are preservation-only: they are never exposed, but their raw bytes survive moves and field edits. One source session targets one canonical record; another record in the same physical JSON file reports a source-lock conflict, while the same canonical record reached through another lens attaches to the existing session.

Apply, collection Add/Remove/Move/Replace, Restore Original Order, Undo, Redo, and Preview are in-memory. Session-generated tutorial item keys and effect-row keys preserve duplicate identity and original index during the session; they are not persisted or remapped after reload. A moved unchanged effect row reuses its exact raw object bytes. Editing an existing row patches only changed direct child tokens inside that raw object. Removing a row omits only that child span; adding a row serializes only `effect`, `target`, and `amount` in deterministic descriptor order with local compact/multiline, newline, indentation, and colon-spacing style. The containing Upgrade and full file are never reserialized. Every collection operation and Undo/Redo rechecks the source revision. Reference targets are re-resolved against the selected pack and revalidated during Apply, Preview, and Commit. A missing target, changed source claim, cross-pack key, non-Passive upgrade, invalid effect/target/amount, invalid line count/text, unsupported row, or stale source blocks the transaction without substituting a fallback. Preview rejects stale bytes/path/GUID/manifest-source-list revisions and validates the proposed complete selected pack by substituting the proposed source text into the existing strict Survivors validator without writing it. Errors block save and warnings require explicit confirmation. Cancel before commit restores nothing because it changes exactly zero source bytes and creates no backup.

Commit writes a flushed temporary file beside the destination and uses platform `File.Replace`; it never deletes the destination first. Exact original bytes and transaction metadata are prepared under `Library/Deucarian/GameContentAuthoring/Recovery/Survivors` before replacement. The backend then synchronously reimports, verifies the exact new hash and edited values, reruns strict pack validation, and reindexes the selected pack. An evolution-reference edit replaces only the selected prerequisite string token; a tutorial edit replaces only the complete direct `lines` array token; an Effects edit replaces only the complete direct `effects` array token. Array serialization JSON-escapes mapped strings, preserves compact or multiline local style, derives local newline/indentation when needed, and leaves every byte before and after that array span unchanged. Basic and Neon use different GUIDs, paths, lock keys, and pack-scoped record keys even where logical record IDs match. Invalid staging in one pack neither writes nor invalidates the other pack.

`File.Replace` failures retain the operation stage, safe project-relative destination, exception type, full `HResult`, available low-word Windows code, attempt number, and final retry disposition in the transaction result. Only `IOException` replacement failures with Windows code `32` (`ERROR_SHARING_VIOLATION`), `33` (`ERROR_LOCK_VIOLATION`), or `1175` (`ERROR_UNABLE_TO_REMOVE_REPLACED`) are retryable. The transaction makes at most four total attempts with `25`, `75`, and `200` millisecond delays, so retry delay never exceeds `300` milliseconds. Access denial, `UnauthorizedAccessException`, codes `1176`/`1177`, unknown codes, validation, path/security, import, and verification failures are never retried.

Before each retry, the backend proves the session is still active, the destination retains the expected exact hash, the one prepared temporary file retains the immutable proposed hash, and the selected source still has the expected target, path/GUID, ordered manifest source list, pack/record identity, and revision. A failed check stops immediately and enters the same original/proposed/unknown-hash recovery classification as any other failed transaction. Commit, explicit rollback, and automatic exact-byte restoration use this rule. AssetDatabase import remains strictly after successful replacement and never runs between attempts.

The unique-file atomic support probe uses the same retry budget. Successful support and permanent nonretryable failures remain cached. Exhausted transient replacement failures are held only for a one-second cooldown, after which a fresh probe may recover; this prevents repeated UI probes from hammering a temporarily busy filesystem without making the imported sample permanently read-only. The retained historical failures do not identify which external process held the Windows handle, so the template does not claim an antivirus, indexer, Unity refresh, or another specific lock owner.

Rollback after a successful commit first requires the current source to match the committed revision, then atomically restores and verifies the exact previous bytes. It refuses to overwrite a later external edit. Up to five resolved backups per physical source are retained; unresolved recovery records are retained for explicit review. In-session Undo/Redo does not promise cross-session Unity Undo.

Existing standalone ScriptableObject authoring remains separate under the synthetic Project Content pack. JSON remains the only gameplay source of truth and no ScriptableObject mirror is created.

This follows the Template Contract: "Extract only reusable infrastructure, never the playable vertical slice."
