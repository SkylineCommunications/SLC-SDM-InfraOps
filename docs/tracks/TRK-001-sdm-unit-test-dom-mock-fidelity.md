# TRK-001: SDM unit-test DOM mock fidelity

| Field | Value |
|---|---|
| Status | Active |
| Owner | Miguel Garrido |
| Collaboration | [308202](https://collaboration.dataminer.services/myTasks/list/task/308202) |
| Created | 2026-10-28 |

## Goal

Make the in-memory DOM mock used by the SDM unit tests reject what a real DataMiner rejects, so a green test suite is evidence that generated mappers and repositories are correct.

## Why

Generated SDM mappers have shipped with CLR-type bugs (reference fields written as `string` into `Guid` DOM fields) while every in-memory CRUD round-trip stayed green. The mock (`Skyline.DataMiner.Utils.DOM` `DomSLNetMessageHandler`) stores whatever it is given; the real check lives server-side in `ManagerStore.CrudHelperComponent` and surfaces as `DomInstanceSectionInvalidFieldValueTypes`.

## Scope

- Validation gaps in `DomSLNetMessageHandler` as consumed by the `SDM.*.Tests` projects.
- Test-project-local wrappers/decorators over the nuget mock. No fork of Utils.DOM.
- Where a gap is generic, an upstream PR to `SkylineCommunications/Skyline.DataMiner.Utils.DOM` may be proposed as a follow-up item.

## Out of scope

- Fixing mappers or codegen templates that the hardened mock exposes. Those are tracked per domain.
- Server-side validation behaviour.

## Work-items

| ID | Title | Status |
|---|---|---|
| [ITM-001](../items/ITM-001-dom-mock-field-value-type-validation.md) | DOM mock: field-value-type validation on Create/Update/CreateOrUpdate | Done |

## Candidate follow-ups (not yet items)

- Enable `validateAgainstDefinition: true` on the mock in all test projects (requires fixing missing/optional descriptor seeding first).
- Share the validating handler with `SDM.FacilityManagement.Tests`, `SDM.PlanAndBuild.Tests`, `SDM.InfraOpsProperties.Tests`.
- Upstream the type check into Utils.DOM behind the existing `validateAgainstDefinition` flag.
