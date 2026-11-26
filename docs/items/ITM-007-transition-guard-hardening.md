# ITM-007: Transition guard hardening

| Field | Value |
|---|---|
| Status | Done |
| Parent track | [TRK-001](../tracks/TRK-001-sdm-unit-testing.md) |
| Spec | none |
| Collaboration | 310997 |
| Assignee | Tomás Martins |
| Created | 2026-11-25 |
| Links | [ITM-005](ITM-005-generic-transition-validation.md), [ITM-006](ITM-006-generic-transition-validator-base.md), CAN-003 V8 |

## Problem

A review of ITM-005/006 found these issues:

- `UpdateAndTransitionTo` saved the field update before the transition guard ran, so a failing rule left a half-applied change.
- The guard wiring was an untyped `TransitionGuard` delegate per repository, so other entities had no reusable contract to opt into.
- `ITransitionValidator<T, TState>` had no consumers.
- The helper comment claimed that constructor injection would be circular, which is wrong.
- No test covered a failing rule on Asset or through the opt-in mechanism.

## Solution

- Removed `ITransitionValidator<T, TState>`. `TransitionValidatorBase<T, TState>` is the single generic contract for validators.
- Replaced the delegate with a plain `internal TransitionValidator` property on `AssetDomRepository` and `AssetClassDomRepository` (null = no rules).
- `AssetManagementApiHelper` assigns the validators to that property, with a corrected comment.
- `IAssetRepository.EnsureTransitionAllowedInternal` runs these checks:
  - the state machine;
  - the path;
  - the rules.

  `UpdateAndTransitionTo` calls it before `Update`, so it no longer persists half-applied changes.
- Tests use the `BlockingTransitionValidator<T, TState>` test double.

## Acceptance criteria

1. Any entity can opt in by:
   - deriving its validator from `TransitionValidatorBase<T, TState>`;
   - adding an internal `TransitionValidator` property to its repository and checking it before every write;
   - assigning the property in the API helper.
2. When a rule fails, `TransitionTo` does not change the state. This applies to both Asset and Asset Class.
3. When a rule fails, `UpdateAndTransitionTo` persists neither the edit nor the state change.
4. A raw repository with no validator attached behaves as before.
5. `dotnet build` and `dotnet test` on `SLC-SDM-InfraOps.slnx` are green.

## Out of scope

- Opting in other entities. Facility Management first needs `Racks` typed to expose `TransitionTo`.
- Real business rules, since all rules are still empty.
- Unifying exception types between Asset and Asset Class.
