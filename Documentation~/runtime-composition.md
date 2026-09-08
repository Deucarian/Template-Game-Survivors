# Runtime composition

The controller retains its original script GUID, six serialized fields, public methods and diagnostic properties. Existing scenes, sample bootstraps, weapon adapters and debugger callers continue to use that facade. New collaborators are internal to the runtime assembly; tests use explicit friend assemblies rather than adding public extension contracts.

## Owners and boundaries

| Responsibility | Owner | Inputs and effects |
| --- | --- | --- |
| Run phase, elapsed simulation time and endless transition | `SurvivorsRunSession` | Explicit run commands and elapsed delta; no scene dependencies |
| XP budget, level queue, overflow and draft cooldown | `SurvivorsExperienceProgression` | Tuning and XP grants; bounded queue and explicit draft consumption |
| Run, class, relic and persistent stat modifiers | `SurvivorsUpgradeModifiers` | Authored effects in order; health/barrier/magnet side effects through `ISurvivorsUpgradeEffectSink` |
| Profile loading and persistence lifetime | `SurvivorsProfileSession` | Injected storage factory or borrowed persistence; no duplicate storage implementation |
| Audio throttling | `SurvivorsAudioEventRouter` | Caller-supplied time, normalized event IDs and palette throttle |
| Audio resources and playback | `SurvivorsAudioPresenter` | Theme/time providers; owns generated clips and source cleanup |
| Damage feedback, banners and HUD styling | Popup/banner presenters and `SurvivorsHudStyles` | Display data and elapsed time; no combat or progression mutation |
| Draft-card and status drawing | Draft-card/status presenters | Value snapshots, styles and theme; controller retains actions |
| Swarm cadence and population policy | `SurvivorsSwarmSpawnCoordinator` | Tuning, run-flow pressure and a narrow spawn/counter port |
| Timed threat warnings and endless retries | `SurvivorsTimedEncounterDirector` | Run-flow/tuning/time port; emits spawn, warning and victory commands |
| Offscreen spawn geometry | `SurvivorsOffscreenSpawnPolicy` | Seed, distance band and optional visible ground rectangle |
| Unity camera ground projection | `SurvivorsCameraGroundProjection` | Camera rays to ground bounds; separate from geometry policy |
| Enemy status stacking and expiry | `SurvivorsEnemyStatusEffects` | Damage, liveness and tint-refresh callbacks |
| Enemy material, hit flash and pooling presentation | `SurvivorsEnemyPresentation` | Transform and status readers; owns material disposal |
| Death bursts and ranged shot cues | `SurvivorsCombatFeedbackPresenter` | Transform provider and event values; owns separate bounded lists and material lifetimes |
| Incoming-threat and slam ground telegraphs | `SurvivorsThreatTelegraphPresenter` | Transform provider and threat values; owns charge/fade windows and bounded lists |
| Major reward-cache markers | `SurvivorsRewardDropPresenter` | Transform/theme providers and reward values; owns marker animation and cleanup |
| Threat health and offscreen marker read model | `SurvivorsThreatHudModel` | Read-only value observations; owns authored visibility, priority/tie rules, labels and last-marker history |
| Threat HUD actor access | `SurvivorsEnemyHudSource` | Adapts the existing enemy collection into copied observations; does not cache a second enemy collection |
| Threat HUD geometry and rendering | `SurvivorsThreatHudLayout` / `SurvivorsThreatHudPresenter` | Pure viewport/position geometry, then rendering over values, camera projection, theme color and style |
| Horde rush encounter | `SurvivorsHordeRushEncounter` | Schedule, warning/burst scaling, World Spawning member IDs, clear rewards, pulse accounting and Breaker timer; narrow world spawn/damage/feedback port |
| Roaming cache and shrine travel triggers | `SurvivorsTraversalDirector` | Accumulated travel, shrine eligibility and shrine-before-cache command ordering; no scene state or presentation ownership |
| Roaming caches and ambush clears | `SurvivorsRoamingCacheEncounter` | Drop/ambush cadence, active instance IDs, successful-drop accounting and Wayfinder Surge; narrow exploration world commands |
| Shrine trials and clears | `SurvivorsShrineEncounter` | Trial pressure/role selection, active instance IDs, clear rewards and Shrine Surge; preserves distinct failed-drop rules |
| Waystone discoveries and chains | `SurvivorsWaystoneExploration` | Horizontal discovery radius, discovered keys, focus/chain timers, rewards and ambushes; observes landmark positions without owning scene transforms |
| Endless exploration scaling and feedback | `SurvivorsExplorationBonuses` / `SurvivorsExplorationFeedback` | Immutable scaling values derived from victory/tier and a single latest exploration message with explicit presentation callbacks |

The scene controller composes these owners and forwards existing getters. It does not retain a second mutable copy of their state. The encounter ports expose the operations each coordinator needs; they do not expose the controller itself. The existing actor classes remain public and in the same assembly.

## Preserved behavior

- Simulation time advances only while playing; opening drafts and continuing after victory preserve the original run clock rules.
- Modifier effects retain authored order, including health changes that depend on preceding effects. Persistent bonuses apply by delta when reapplied.
- Borrowed persistence is never disposed by the profile session. Owned persistence is released on rebind or shutdown.
- Swarm spawn selection reads the live sequence after each successful spawn, retains capacity blocking, and does not produce catch-up bursts.
- Timed normal threats consume their authored trigger once. Endless threats retry failed spawns and schedule their next interval after a successful spawn.
- Damage-over-time effects retain their original final-whole-tick damage behavior and stop processing later status types when an earlier effect kills the enemy.
- Rebuilding feedback audio now releases its previous generated clips and source; pooling an enemy resets transient status and visual state.

## Validation and limits

Direct EditMode tests exercise the owners without constructing a complete playable world. Existing controller and imported-scene tests continue to cover their composition. Content editing integration tests require exactly one imported current `Basic Survivors Game` sample, including both Basic and Neon Arcana manifests and their referenced assets. Import the package sample before those tests; a package-cache copy alone is not an imported content pack.

The direct tests distinguish scene-free run/XP/modifier/spawn/status policy from Unity presentation lifecycle tests. Presentation cases cover bounded eviction, distinct telegraph expiry, missing presentation roots, parent destruction, owned material/clip release and repeated disposal. They are separate from full controller, content-editing and PlayMode integration coverage.

Threat HUD model tests use value observations without actors or scene construction. They preserve authored flags independently of enemy role, first-in-source order for exact selection ties, role then weakest-health selection, horizontal marker eligibility, the existing full-distance marker tie rule, marker history and small/wide viewport layout. A separate Unity adapter test checks copied positions, live collection changes and destroyed actors. Drawing does not change these selection/history rules.

Horde/traversal tests use fake world command ports without menus, audio or scene hierarchies. They retain blocked-spawn schedule consumption, continued attempts after individual horde spawn failure, member removal before the existing ordered death reward sequence, successful-drop gating of clear rewards, special-drop cadence, pulse/surge timing, travel backlog limits and disabled/active-shrine travel behavior. World Spawning continues to own actual spawned instance lifetime.

Exploration tests cover lane geometry and endless scaling, failed-cache cadence, continued ambush attempts after failure, exact last-member completion, XP-gated cache clear specials, blocked shrine trials, shrine surge on failed drops, one reward per waystone key, horizontal discovery distance and chain cue ordering. Cache, shrine and waystone rules intentionally differ; their owners preserve these rules. Clearing world membership preserves diagnostics, while run restart resets both.

This decomposition is in progress. Player movement/safety, combat augments, full build/draft orchestration, authored binding, arena/compass presentation, major menu/tutorial/result screens, broader HUD text formatting and the diagnostic facade still remain in the controller. Its legacy size is not treated as compliant with the 500-line production-source limit merely because the new collaborators are below that limit. No numbered partial-class split is used.
