# ITM-008: Asset and Asset Class transition rules

| Field | Value |
|---|---|
| Status | Done |
| Parent track | [TRK-001](../tracks/TRK-001-sdm-unit-testing.md) |
| Spec | none |
| Collaboration | 310997 |
| Assignee | Tomás Martins |
| Created | 2026-11-26 |
| Links | [ITM-007](ITM-007-transition-guard-hardening.md), CAN-003 V8 |

## Problem

The transition guard from ITM-005/006/007 had no rules. Two transitions need a database-backed check:

- deprecating an Asset Class that assets still use;
- disposing an Asset that still holds other assets.

## Solution

- `AssetClassValidator.ValidateTransitionRules`:
  - applies when the target state is Deprecated;
  - fails if any non-Disposed asset uses the class;
  - uses the existing `SdmEntityLoader.HasNonDisposedAssetsForAssetClasses`, which counts without loading assets.
- `AssetValidator.ValidateTransitionRules`:
  - applies when the target state is Disposed;
  - fails if any asset is currently placed in its holders (`Location.ParentAsset`);
  - uses the new `SdmEntityLoader.HasChildAssets`, which also only counts.
  - Assets that are only heading there (`DestinationLocation.ParentAsset`) do not block it.
- Both rules run on `TransitionTo`, which throws on failure, and on the public `ValidateTransition`, which returns a result instead.

## Acceptance criteria

1. Deprecating an Asset Class used by a non-Disposed asset throws `ValidationException`, and the state is unchanged.
2. Deprecating an Asset Class whose only assets are Disposed succeeds.
3. Disposing an Asset that has a child in a holder throws `AssetTransitionValidationException`, the state is unchanged, and `ValidateTransition` returns invalid.
4. Disposing an Asset with no children succeeds.
5. `dotnet build` and `dotnet test` on `SLC-SDM-InfraOps.slnx` are green.

## Out of scope

- Blocking disposal for incoming (destination) children.
- Rules for other target states or other entities.
