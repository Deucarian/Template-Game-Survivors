# Deucarian Template Game Survivors Agent Notes

Package ID: `com.deucarian.template.game.survivors`
Repository: `Deucarian/Template-Game-Survivors`

Follow the canonical Deucarian governance docs in [Package Registry](https://github.com/Deucarian/Package-Registry/blob/develop/ARCHITECTURE.md), especially capability ownership and dependency rules.

## Ownership

This package owns:

- A playable Survivors-style horde roguelite template slice, local genre-kit adapters, local weapon archetype composition, local meta-progression/persistence wiring, sample bootstrap code, and template-specific content validation.

Registered capabilities:
- None.

This package must not own:

- Reusable Survivors framework infrastructure, Combat rules, Attack/Projectile/Weapon reusable internals, Encounter foundations, Persistence/Progression core behavior, Package Installer behavior, or registry governance.

## Dependencies

Allowed dependency shape:

- Template package may depend on lower reusable gameplay packages needed by its playable sample and local adapters.

Required dependencies and why:

- `com.deucarian.attacks`: local weapon/attack adapter definitions.
- `com.deucarian.common`: approved transient Unity object cleanup for local template runtime objects.
- `com.deucarian.combat`: damage and combat state.
- `com.deucarian.encounters`: horde/wave foundations.
- `com.deucarian.game-content-authoring`: editor validation/content authoring hooks.
- `com.deucarian.gameplay-foundation`: shared IDs and deterministic primitives.
- `com.deucarian.persistence`: local save/load and Unity persistence helpers.
- `com.deucarian.progression`: rewards, classes, and meta progression foundations.
- `com.deucarian.projectiles`: projectile weapon adapter foundations.
- `com.deucarian.run-upgrades`: level-up draft and upgrade effect adapters.
- `com.deucarian.weapon-systems`: weapon slot and fire cadence foundations.
- `com.deucarian.world-spawning`: horde spawn/presentation adapters.
- `com.unity.modules.particlesystem`: built-in Particle System module used by runtime feedback effects.

Optional/version-defined dependencies:

- None.

Architecture exceptions:

- Editor content validation writes validation summaries to the Unity console for template developer visibility.

## Policies

- Keep reusable systems in lower packages; template code may compose and demonstrate them but should not become a generic framework.
- Local genre-kit code can be pragmatic, but reusable behavior should be proposed to the owning package before extraction.
- Template Contract: do not hollow out the template. Preserve the playable vertical slice, do not extract packages unless the Basic Survivors sample still plays, do not replace authored content with hardcoded runtime content, and do not use package extraction as an excuse to remove sample gameplay.
- Do not add UI, monetization, networking, or broader framework dependencies unless the template actually ships those flows.
- Logging: Direct Unity Debug calls are limited to editor/template validation diagnostics listed in `deucarian-package.json`.
- Unity object lifetime: Use Common's `UnityObjectUtility.DestroySafely` for local template runtime cleanup.
- Testing: Test fixture teardown may use Unity `Destroy`/`DestroyImmediate` directly.

## Validation

Run the shared validator before committing:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

Also run existing repository tests when changing code or asmdefs. Documentation-only updates should still run `git diff --check`.

## Codex Guidance

- Inspect current files before changing anything.
- Work on `develop`; do not edit or merge `main` unless the task is promotion-only.
- Do not edit `Library/PackageCache`.
- Do not guess package versions or dependency versions.
- Do not add package dependencies casually; update asmdefs, `package.json`, `deucarian-package.json`, Package Registry, Package Installer fallback, and Bootstrap fallback together when a dependency is truly required.
- Do not create local copies of shared helpers.
- Keep commits focused and report exactly what changed and what was validated.
