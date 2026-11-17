# TRK-001: SDM unit testing

| Field | Value |
|---|---|
| Status | Active |
| Owner | Miguel Garrido |
| Created | 2026-10-28 |

## Goal

A green SDM unit-test suite is trustworthy evidence that the models, mappers, repositories and validators work against a real DataMiner.

## Why

SDM mappers have shipped with CLR-type bugs (reference fields written as `string` into `Guid` DOM fields) while every in-memory CRUD round-trip stayed green. The in-memory DOM mock (`Skyline.DataMiner.Utils.DOM` `DomSLNetMessageHandler`) stores whatever it is given; the real check lives server-side in `ManagerStore.CrudHelperComponent` and surfaces as `DomInstanceSectionInvalidFieldValueTypes`. Mock fidelity is the first gap; more may follow.

## Scope

- Fidelity of the test doubles used by the `SDM.*.Tests` projects (today: the DOM mock).
- Test-project-local wrappers and decorators over the NuGet mock. No fork of Utils.DOM.
- Where a gap is generic, an upstream PR to `SkylineCommunications/Skyline.DataMiner.Utils.DOM`.

## Out of scope

- Fixing mappers or codegen templates that a hardened mock exposes. Those are tracked per domain.
- Server-side validation behaviour.

## Work-items

| ID | Title | Status | Collaboration |
|---|---|---|---|
| [ITM-001](../items/ITM-001-dom-mock-field-value-type-validation.md) | DOM mock: field-value-type validation on Create/Update/CreateOrUpdate | Done | [308202](https://collaboration.dataminer.services/myTasks/list/task/308202) |
| [ITM-004](../items/ITM-004-asset-holder-hierarchy-role.md) | Asset HolderNumber validation must consider Hierarchy Role | Done | TBD |
## Candidate items (not yet created)

- Enable `validateAgainstDefinition: true` on the mock in all test projects (requires fixing missing/optional descriptor seeding first).
- Share the validating handler with `SDM.FacilityManagement.Tests`, `SDM.PlanAndBuild.Tests`, `SDM.InfraOpsProperties.Tests`.
- Upstream the type check into Utils.DOM behind the existing `validateAgainstDefinition` flag.

## Related canon

- [CAN-001](../canon/CAN-001-dom-structure.md): DOM structure and field rules.
- [CAN-003](../canon/CAN-003-repository-and-middleware-pipeline.md): what the repositories and validators under test do.
