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
| Infinite arena and waypoint observations | `SurvivorsArenaPresenter` / `SurvivorsArenaGeometry` | Owns tile/landmark hierarchy and recoloring, deterministic cell positions/keys, copied landmark observations and nearest-target read query |
| Waystone compass drawing | `SurvivorsWaystoneCompassPresenter` | Target distance/direction and elapsed time; owns arrow hierarchy, visibility, orientation, pulse and theme refresh |
| Generated arena materials | `SurvivorsArenaPrimitives` | Creates local visual primitives and releases their materials on rebuild/disposal, including after parent loss |
| Player vitality and safety | `SurvivorsPlayerVitals` | Owns HealthState, barrier, contact/dash safety, one-use clutch response, pickup healing and damage accounting; uses Combat's existing resolver |
| Player movement and dash cadence | `SurvivorsPlayerMotion` | Analog input, bounded motion/safety/travel/pressure ports, cooldown and dash diagnostics; world transform adapter remains in composition |
| Dash path pressure | `SurvivorsDashPressure` | Snapshot of existing enemy references, segment projection, ordered damage/death/knockback behavior and per-hit accounting callback |

| Active run build | `SurvivorsRunBuildState` | Owns class-filtered catalog, rank state, first-entry metadata, acquired passive/evolution sets and slot/prerequisite eligibility; typed acquisition callbacks |
| Draft rarity profiles | `SurvivorsDraftRarityPolicy` | Live authored early/mid/late/reward weights, luck floors and weighted catalog copies with original upgrade effects |

| Draft offer pools | `SurvivorsDraftCatalogPolicy` | Eligible weighted pools, reward rarity fallback and read-only evolution-primer queries |
| Guaranteed draft choices | `SurvivorsDraftGuaranteePolicy` | Rarity/primer/early-passive priority, duplicate suppression, catalog-ordered evolution locks and stable seed salts |
| Deterministic offer generation | `SurvivorsDraftOfferGenerator` / `SurvivorsDraftProgress` | Explicit run-progress values plus build/rarity policies; produces offers without applying upgrades or spending charges |

| Active draft selection and continuation | `SurvivorsDraftSession` | Owns offers, reroll/banish/skip charges, timer, diagnostics and ordered victory/relic/queued-XP continuation; bounded feedback/application port |
| Acquired relic inventory | `SurvivorsRelicInventory` | Sole ordinal selected-ID set and ordered selected list; reserves identity before effects and publishes count/order before acquisition feedback |

| Menu navigation and tutorial progression | `SurvivorsMenuSession` | Sole visibility/tab/step state, build input gates and explicit persisted tutorial commands |
| Tutorial fallback content | `SurvivorsTutorialContent` | Bounded step indexing and template copy, with themed text supplied by the read adapter |
| Build and tutorial screens | `SurvivorsBuildMenuPresenter` / `SurvivorsTutorialPresenter` | Render menu state, prepared text, theme and styles; build presenter owns its scroll position |
| Full-screen layout | `SurvivorsScreenLayout` | Pure viewport panel sizing and shared IMGUI fill primitives |

| Run-mode and theme selection screen | `SurvivorsRunModePresenter` / `SurvivorsRunModeCardView` | Copies authored mode text and renders available themes through explicit start/theme/tutorial commands |
| Full draft screen | `SurvivorsDraftScreenPresenter` / `SurvivorsDraftScreenLayout` | Prepared card models, bounded session commands and responsive card/scroll/selection geometry |
| Victory/defeat and result options | `SurvivorsRunResultPresenter` / result read values | Copied summary, class and purchase labels; explicit continuation/restart/class/purchase commands |

| Kill streak reward cadence | `SurvivorsKillStreakRewards` | Owns streak window, successful drop counters, tiered Tempo Surge and independent clocks; bounded pickup/feedback port |
| Experience combo and Gem Rush | `SurvivorsExperienceComboRewards` | Owns combo budget, once-per-combo activation and banner; gameplay and presentation clocks remain distinct |

| Enemy spatial queries and reentry | `SurvivorsEnemySpatialQueries / SurvivorsEnemyNavigation` | Borrowed existing actor list, exact distance/crowd ties, leash/catch-up policy and single reentry counters |

| Draft card projection and upgrade previews | `SurvivorsDraftCardFactory / SurvivorsUpgradePreviewFormatter` | Borrowed RunBuild, typed current stat values and theme; two-effect card previews preserve source order |

| Build acquisition milestones and timed rewards | `SurvivorsBuildSurgeRewards` | Authoritative build membership, one-use slot milestones, evolution recalls and repeated relic pulses through bounded commands |

| Build HUD content and drawing | `SurvivorsBuildContentLabels / SurvivorsBuildHudModel / SurvivorsBuildHudPresenter` | Live loadout/catalog/ranks, lazy evolution objective, prepared pickup/relic labels and bounded panel drawing |

| Persistent purchases, class selection and run reward transactions | `SurvivorsPersistentProgression / SurvivorsRunRewards` | Borrowed profile and authored content; sole bonus/grant counters and successful terminal idempotence; explicit feedback commands |

| Endless threat and selected-card payouts | `SurvivorsEndlessSurgeRewards / SurvivorsDraftSelectionRewards` | Post-victory escalation, timed bonuses, jackpot distribution and successful drop accounting; bounded world commands |

| Generated world, spawning and camera resources | `SurvivorsRuntimeWorld / SurvivorsRuntimeCamera` | Owned hierarchy/source materials and existing WorldSpawnService; borrowed pose policy; generated camera/listener lifetime separate from run restarts |

| Authored binding and runtime content factories | `SurvivorsContentBinding / SurvivorsRuntimeContentResolver` | Single strict/fallback status and bound definition, explicit tuning/profile/theme commands, borrowed authored factories and last-created-flow observation |

| Enemy defeat transaction | `SurvivorsEnemyDefeatFlow` | Sole kill and role counters, world removal before ordered XP/death/drop/draft/encounter completion commands; live actor observations retain pool-reset behavior |

| Run summary, evolution and milestone text | `SurvivorsRunSummaryModel / SurvivorsEvolutionHudModel / SurvivorsRunMilestoneModel` | Copied run observations and borrowed build queries, stable summary list identity, phase-gated milestone reads and deterministic display ties |

| Pickup collection and major reward caches | `SurvivorsPickupCollection / SurvivorsMajorRewardPickupCache` | Single existing pickup list, collection dispatch, attraction and pulse schedule, cache/drop counters and explicit reward/feedback/despawn order |

| Enemy damage and combat augmentation | `SurvivorsEnemyDamage / SurvivorsDamageAugments` | Owned Combat catalog and deterministic critical RNG, live modifier values and status/evolution decisions through narrow target commands |

| Payload hazard chains and death novas | `SurvivorsPayloadHazardRewards / SurvivorsDeathNova` | Independent snare window/cooldown, successful-drop accounting and two-pass live-target damage over the borrowed actor list |

| Player HUD and build menu text | `SurvivorsPlayerHudTextModel / SurvivorsBuildMenuTextModel` | Copied current stats/run labels and selected-tab-only projection; no rendering or gameplay commands |

| First events and observed minute checkpoints | `SurvivorsRunTelemetry` | One event-time and checkpoint store, explicit event commands and observed level captures; no summary formatting |

| Major threat abilities and support spawning | `SurvivorsMajorThreatAbilities / SurvivorsEnemySupportSpawning` | Enrage membership and diagnostics, role-specific slam/support policy, live capacity and shared sequence reads through narrow spawn commands |

| Frame input priority | `SurvivorsFrameInput` | Keyboard adapter, debug visibility and explicit phase/menu command routing; run and menu state remain borrowed |

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

Arena tests cover positive/negative cell boundaries, signed cell-key identity, landmark offsets, horizontal nearest-target selection, discovered-target suppression and exact tile/landmark counts and positions. Rebuilding the arena replaces its previous hierarchy and releases generated materials; disposal remains safe after the parent has already been destroyed. Presentation reads discovered keys and does not award exploration rewards.

Player tests cover barrier absorption starting contact safety, blocked damage feedback, exactly one clutch on threshold crossing, damage accounting before defeat, actual pickup healing and barrier caps, analog/diagonal movement, dash travel-before-safety-before-pressure ordering, pause/cooldown behavior and segment endpoints. Maximum-health effects retain their existing double precision input. The owner uses the shared Combat resolver and does not duplicate damage formulas.

Build policy tests preserve first matching class gates and the all-rejected catalog fallback, duplicate metadata precedence, full-slot rank upgrades, one passive acquisition callback, weapon ownership checks, reduced evolution rank and passive prerequisites, acquisition callback order and reset boundaries. Rarity tests cover late-before-mid thresholds, live tuning/luck, disabled rarity weights, common floors and effect-preserving catalog copies.

Offer tests cover reward-pool fallback, rarity-before-primer guarantees and duplicate locks, missing-passive requirements, evolution lock precedence and source order, explicit normal versus derived reward locks, deterministic rerolls, disabled pools and each reward seed salt.

Draft-session tests cover charge spending only after successful generation, failed banish regeneration without skip rewards, timeout attempt accounting when eligibility changes, first victory versus endless boss rewards, miniboss/relic/queued-level ordering, empty relic fallback, empty normal-pool compensation and clear versus reset. Relic tests check identity reservation before effects, acquired-order visibility and duplicate rejection.

Menu tests preserve build-toggle priority, non-playing close behavior, tutorial reset/persistence/audio ordering, closing or advancing while hidden, first-run gating and fallback text indexing. Pure panel tests cover wide, narrow and very small viewports, including the legacy 220-pixel lower bound. Actual screen rendering remains covered by shared integration and visual validation.

Screen-model tests verify wide/narrow draft orientation, card spacing and scroll extent, separate relic and upgrade selection regions, tiny viewport bounds and copied authored mode text/title fallbacks. Result class rows refresh after selection so subsequent rows reflect the updated selection in the same draw.

Pickup rhythm tests preserve cadence under failed spawns, tempo tier caps, independent streak/surge expiry, combo-versus-gameplay pause timing, repeated pickup refresh, reset boundaries and disabled/live-clamped bonuses.

Eighteen cases retain target/radius/crowd ordering, leash and reentry behavior. Dead fixtures use Combat LifeState.Dead.

Fourteen cases cover metadata/category labels, exact first-two effect previews, live stat comparison, rarity and relic presentation.

Six cases cover slot gates, disabled pulses, source order, successful recall accounting, independent timers and live bonuses.

Nine cases preserve authored labels, three rank fragments, source-order full rows, lazy objective queries and narrow/18-row rendering preparation.

Twenty-three cases preserve purchase/class options, active-run gates, callback publication, role rewards, scaling, first-victory bonuses and success-only terminal idempotence.

Five cases cover victory/role gates, failed drops, persistent tier and capped intensity, reward rarity gates and separate particle/audio choices.

Ten cases cover hierarchy/palette/pool definitions, service-root teardown, owned material release and generated versus borrowed camera/listener lifetime. Trail alpha checks Unity Color32 quantization.

Ten cases cover strict required assets, Basic/Neon binding, callback order, failed theme rollback, active-run behavior and fallback/rebind diagnostics.

Seven cases preserve release and reward order, role-specific draft fallback, endless decisions, diagnostics and the post-despawn splitter label read.

Eighteen cases preserve summary order and fallback text, clear versus rebuild, evolution readiness, milestone epsilon/tie rules and live status labels.

Eighteen cases cover cache distribution and failure accounting, reward-before-removal/despawn, XP metric/gain/combo order, recall membership and independent diagnostics/pulse resets.

Twelve cases preserve critical RNG consumption, damage-source classification, heal/barrier/status/execute ordering, named evolved effects and destroyed-Unity-target guards before interface conversion.

Five cases cover disabled and failed rewards, chain timing, horizontal body-radius capture, mutation during feedback, repeated liveness checks and destroyed actor targets.

Eight cases preserve row ordering, metric precision, active surge labels, authored mode/class text and controls copy.

Three cases preserve pre-start zero values, reset-to-unseen behavior, first-event locking at zero and all crossed minute checkpoints without interpolation.

Twenty-five cases preserve Playing versus terminal-only gates, full-distance slam hits, actual health/barrier accounting, failure-sensitive seed order and separate membership/diagnostic resets.

Six cases preserve F1 priority, tutorial/build interception, draft presentation then timeout/input, failed purchase fallthrough and movement/dash/magnet ordering.

This decomposition is in progress. The remaining controller policies and compatibility facade aggregate are still being separated. The legacy controller is not treated as compliant with the 500-line production-source limit merely because new collaborators are below that limit. No numbered behavior split is used.
