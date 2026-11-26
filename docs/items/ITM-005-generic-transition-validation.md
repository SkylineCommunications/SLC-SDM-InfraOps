# ITM-005: Generic transition validation for Asset and Asset Class

| Field | Value |
|---|---|
| Status | Done |
| Parent track | [TRK-001](../tracks/TRK-001-sdm-unit-testing.md) |
| Spec | none |
| Collaboration | 310997 |
| Assignee | Tomás Martins |
| Created | 2026-11-25 |

> The `TransitionGuard` delegate was replaced by an internal `TransitionValidator` property in [ITM-007](ITM-007-transition-guard-hardening.md).

## Problem

Create/Update/Delete are validated by the middleware, but `TransitionTo` bypasses it. A transition only checks the state-machine path (`StateMachine.IsTransitionAllowed` + `AssetTransitionValidator.ValidatePath`), so business rules that need database reads never run on a state change (CAN-003 V8).

## Solution

- `AssetValidator.ValidateTransition` and `AssetClassValidator.ValidateTransition`: public, non-throwing pre-check (path + business rules) for UIs and IAS.
- `TransitionGuard` (internal) on `AssetDomRepository` and `AssetClassDomRepository`, invoked right before `ExecuteStateTransition`. Wired in `AssetManagementApiHelper` to `EnsureTransitionAllowed` after the validators exist.
- `ValidateTransitionRules` is the extension point for rules. It is empty on master.

## Acceptance criteria

1. `ValidateTransition` returns an invalid result (no throw) for a disallowed transition and a valid result for an allowed one (Asset and Asset Class).
2. A throwing `TransitionGuard` blocks the transition; the stored state is unchanged.
3. `UpdateAndTransitionTo` and `TransitionAndUpdate` reach the guard through `TransitionToInternal`.
4. Existing tests expecting `InvalidOperationException` "State transition from … is not allowed." stay green.
5. `dotnet build` and `dotnet test` on `SLC-SDM-InfraOps.slnx` are green.

## Out of scope

- Business rules (R8 Shelf, R10 Asset Class Deprecated with non-Disposed assets). R10 would break RT `RT_AssetManager_DeleteInstances\DeletionCases.cs:402`.
- `UpdateAndTransitionTo` keeps the update when the guard fails (pre-existing behaviour).
