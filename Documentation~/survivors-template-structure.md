# Survivors Template Structure

## Runtime Kit

`Runtime/SurvivorsTemplateController.cs` contains the first-slice composition root, local actors, and local genre adapters:

- `SurvivorsTemplateController`
- `SurvivorsEnemyActor`
- `SurvivorsProjectileActor`
- `SurvivorsPickupActor`
- `SurvivorsSpawnPoseResolver`
- `BasicSurvivorsGame`

This is reusable Survivors template-kit code, not concrete product content.

## Sample Content

`Samples~/BasicSurvivorsGame` contains a tiny concrete sample game:

- `Scenes/BasicSurvivorsGame.unity`
- `Scripts/BasicSurvivorsGameBootstrap.cs`
- `Content/DefaultEnemies/enemies.json`
- `Content/DefaultWeapons/weapons.json`
- `Content/DefaultUpgrades/upgrades.json`
- `Content/DefaultPickups/pickups.json`

These files are examples for product-owned content flipping. They are not intended to become shared package code.

## Tests

- `Tests/EditMode` covers descriptors, draft choice determinism, spawn flow, weapon death/drop, XP collection, level-up choice, and magnet recall.
- `Tests/PlayMode` covers first playable runtime boot and death/restart.
