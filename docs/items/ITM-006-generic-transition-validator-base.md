# ITM-006: Generic transition validator base

| Field | Value |
|---|---|
| Status | Done |
| Parent track | [TRK-001](../tracks/TRK-001-sdm-unit-testing.md) |
| Spec | none |
| Collaboration | 310997 |
| Assignee | Tomás Martins |
| Created | 2026-11-25 |

> Superseded in part by [ITM-007](ITM-007-transition-guard-hardening.md): `ITransitionValidator` was removed and the `TransitionGuard` delegate was replaced by an internal `TransitionValidator` property.

## Problem

ITM-005 added transition validation to `AssetValidator` and `AssetClassValidator` as duplicated, entity-specific code. Other entities with status transitions (Facility Management entities, Plan & Build Job) cannot reuse it. Connection has no status and is not a candidate.

## Solution

- `ITransitionValidator<T, TState>` and `TransitionValidatorBase<T, TState> : ValidatorBase<T>` in `SDM.InfraOps.Core`.
- The base implements `ValidateTransition` (null → null-entity result, same state → valid, path → rules) and `EnsureTransitionAllowed` (rules only, throws).
- Derived validators supply `GetState` and `ValidateTransitionPath`, and may override `ValidateTransitionRules` and `CreateTransitionException`.
- `AssetValidator` and `AssetClassValidator` derive from the base. `EnsureTransitionAllowed` becomes public (interface member).

## Acceptance criteria

1. `AssetValidator` and `AssetClassValidator` derive from `TransitionValidatorBase` with no duplicated transition logic.
2. Behaviour is unchanged: the ITM-005 tests stay green; Asset guard failures throw `AssetTransitionValidationException`, Asset Class guard failures throw `ValidationException`.
3. `dotnet build` and `dotnet test` on `SLC-SDM-InfraOps.slnx` are green.

## Out of scope

- Opting in other entities (Facility Management, Plan & Build Job).
- Business rules.
