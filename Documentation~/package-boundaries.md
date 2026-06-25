# Package Boundaries

## Existing Package Usage

- `Combat`: health state and damage resolution.
- `World Spawning`: pooled enemy, pickup, and projectile object lifecycle.
- `Weapon Systems`: stable weapon descriptor for the first auto-fire projectile weapon.
- `Projectiles`: stable projectile descriptor for the first projectile.
- `Run Upgrades`: deterministic draft generation, rank tracking, and max-rank filtering.
- `Encounters`: first-slice encounter/wave descriptors for authored spawn-flow shape.
- `Progression`: local Survivors meta currency, legacy XP track, and ranked persistent upgrade purchase state.
- `Persistence`: local Survivors meta profile save/load and schema migration.

## Local Survivors Kit Code

The following intentionally stay local in Phase 2Q:

- top-down player movement
- camera follow
- radial spawn position rules
- run timer and run phase progression
- swarm escalation tuning
- miniboss and boss timing rules
- victory state machine
- boss/miniboss sample profiles and reward-drop rules
- run result summary rules
- boss/miniboss reward bonus rules
- blood shard and legacy XP reward calculation
- persistent meta upgrade effects
- local meta profile DTO shape and migration mapping
- nearest-enemy auto-targeting
- projectile steering and collision checks
- hitscan/beam targeting and primitive beam visuals
- projectile pierce counters
- projectile chain target selection
- projectile fork/split spawn rules
- projectile return/boomerang movement
- grenade throw arcs and fuse timing
- trap/mine placement rules
- payload arming, lifetime, and proximity trigger logic
- payload area overlap and hazard tick rules
- orbit blade motion and contact cadence
- melee arc overlap rules
- burst nova sequencing and pulse visuals
- XP pickup attraction
- magnet recall
- level-up overlay
- concrete upgrade effect application
- authored Survivors sample-library validation

## Package Expansion Candidates Later

Do not extract these yet. Revisit only after parity gaps repeat across real templates or products:

- generalized horde escalation
- boss/miniboss encounter descriptors only after another template proves the same cadence/reward shape
- victory/miniboss reward hooks only after repeated reuse
- pooled projectile impact adapters and impact VFX hooks
- richer Survivors weapon archetype descriptors
- beam/hitscan adapters only after another game proves the same shape
- projectile modifier descriptors for pierce, chain, fork, and return only after repeated reuse
- payload descriptors for grenade/trap/mine arming, triggers, hazards, bounce, clusters, and chain reactions only after repeated reuse
- pooled payload and ground-hazard lifecycle only if another template needs the same runtime contract
- XP/drop lifecycle abstractions
- content validation for authored libraries if multiple templates need the same rules
- shared reward-draft abstractions only if another Survivors-style template proves the same shape
- richer meta progression save DTOs only if multiple concrete games share the same persisted profile model
- diagnostics for spawn pressure, pool stats, and draft state

No shared package extraction happened in Phase 2Q.
