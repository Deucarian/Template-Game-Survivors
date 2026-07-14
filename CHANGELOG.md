# Changelog

## Unreleased

- Bridged Basic Survivors and Neon Arcana JSON records into reusable Attacks, Enemies, Wave / Encounter, Weapon / Tower, Upgrade subtype, and All Content lenses while preserving one canonical identity per source record.
- Added editor-only typed projections for authored weapon, enemy, run-flow, and upgrade values; both 251-record packs remain isolated and JSON-backed, with unsupported fields and categories read-only.
- Added read-only Game Content Authoring manifests, pack-scoped JSON projections, reference navigation, selected-source validation, Standard/Sprint launch actions, and dynamic imported-scene test resolution for Basic Survivors and Neon Arcana.
- Enabled imported-copy editing for approved direct weapon, projectile, and enemy scalar fields through lossless token replacement, strict proposed-pack validation, stale-source protection, atomic replacement, exact recovery backups, reimport/reindex, and revision-checked rollback.
- Enabled canonical same-pack editing of an evolution's required Passive prerequisite through the shared record selector, lossless one-token JSON replacement, target revalidation, strict Passive-category validation, and Basic/Neon isolation.
- Enabled ordered editing of existing Basic and Neon tutorial `Lines` collections with 1-3 nonblank rows, session item identity, localized lossless array replacement, strict proposed-pack validation, atomic recovery, exact rollback, and runtime order/text verification.
- Hardened Survivors JSON transactions with structured `File.Replace` failure evidence, four-attempt retries limited to Windows sharing/lock errors `32`, `33`, and `1175`, exact destination/temp plus source-revision checks before retry, bounded `25`/`75`/`200` ms delays, and a recoverable one-second support-probe cooldown.

## [0.1.0] - 2026-07-01

### Added

- Initial Survivors template package with playable sample, runtime adapters, editor validation, tests, and package validation metadata.
