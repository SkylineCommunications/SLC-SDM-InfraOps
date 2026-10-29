# ITM-001: DOM mock: field-value-type validation on Create/Update/CreateOrUpdate

| Field | Value |
|---|---|
| Status | Done |
| Parent track | [TRK-001](../tracks/TRK-001-sdm-unit-test-dom-mock-fidelity.md) |
| Spec | [SPC-001](../specs/SPC-001-dom-mock-field-value-type-validation.md) |
| Collaboration | [308202](https://collaboration.dataminer.services/myTasks/list/task/308202) |
| Assignee | Miguel Garrido |
| Created | 2026-10-28 |

## Problem

The unit-test DOM mock (`DomSLNetMessageHandler`, `Skyline.DataMiner.Utils.DOM`) does not compare a written `FieldValue`'s CLR type against the `FieldDescriptor.FieldType` registered for that section. A mapper that writes `string` into a `Guid` field passes every in-memory CRUD test because `FromInstance` parses the string back. Real DataMiner rejects the same write with `DomInstanceSectionInvalidFieldValueTypes`.

## Current state (verified 2026-10-28)

- `SDM.AssetManagement.Common\Mock\AssetManagementMockMessageHandler.cs` already seeds typed section definitions for every connection section. `SourcePort`, `SourcePortType`, `DestinationPort`, `DestinationPortType`, `CableType` are `typeof(Guid)`.
- `SDM.AssetManagementTests\ConnectionHelper.cs` constructs `new DomSLNetMessageHandler()`; `validateAgainstDefinition` defaults to `false`.
- Pinned nuget: `Skyline.DataMiner.Utils.DOM` 10.5.2.6 (`Directory.Packages.props`). Latest: 10.6.1.1. Neither the pinned DLL nor `main` compares value CLR types; `ValidateInstance` only checks definition, section definition, descriptor existence and required fields. **Open question from the task is resolved: upgrading does not satisfy this item.**
- The reflection-based `Mapping_ShouldWriteGuidReferenceFields_AsGuid` test is already gone. Nothing to delete.
- `ConnectionDomRepository.g.cs` currently writes `AddOrUpdateValue<Guid>` for the reference fields (the stopgap). Out of scope here.

## Deliverable

A test-project-local decorator over `DomSLNetMessageHandler` that, for `ManagerStoreCreateRequest<DomInstance>`, `ManagerStoreUpdateRequest<DomInstance>` and `ManagerStoreBulkCreateOrUpdateRequest<DomInstance>`, validates each non-empty `FieldValue` against the registered `FieldDescriptor.FieldType` and throws a descriptive exception on mismatch. Wired into `SDM.AssetManagementTests` by default.

## Acceptance criteria

1. **Negative CRUD test exists and fails for the wrong type.** A test in `SDM.AssetManagementTests\Connections\` pushes a raw `DomInstance` whose `SourceInfo.SourcePort` holds a `string` through the mocked connection and asserts an exception whose message names the section, the field, the expected type (`System.Guid`) and the actual type (`System.String`).
2. **Positive CRUD path stays green.** `Create_WithValidConnection_ShouldPersistAndReadBack` and the rest of the Connections filter pass unchanged.
3. **Regression proof.** Temporarily reverting `ConnectionDomRepository.g.cs` to write `string` for `SourcePort` makes `Create_WithValidConnection_ShouldPersistAndReadBack` fail with the type-mismatch exception. Performed and reported, not committed.
4. **Opt-in by definition.** Sections with no registered `SectionDefinition` and fields with no registered `FieldDescriptor` are not validated. Unset/null values are skipped.
5. **No new dependencies, no Utils.DOM fork.** Only existing `Skyline.DataMiner.Utils.DOM` primitives are used.
6. **Full suite.** `dotnet test SDM.AssetManagementTests` passes. The other test projects are untouched and still pass.

## Result (2026-10-29)

| Criterion | Evidence |
|---|---|
| 1 | `ConnectionDomStorageProvider_FieldTypeTests.Create_WithStringInGuidField_ShouldThrowTypeMismatch` passes; message asserts `Source Info`, `Source Port`, `System.Guid`, `System.String`. |
| 2 | `dotnet test SDM.AssetManagementTests --filter FullyQualifiedName~Connections` → 99 passed, 0 failed. |
| 3 | `Source.Port` write changed to `AddOrUpdateValue<string>(..., .ToString())` → 16/16 `ConnectionDomStorageProvider_CRUDTests` fail; `Create_WithValidConnection_ShouldPersistAndReadBack` throws `DomFieldValueTypeMismatchException: DomInstanceSectionInvalidFieldValueTypes: ... section 'Source Info' ... field 'Source Port' ... expects 'System.Guid' but got 'System.String'`. Mapper reverted via `git checkout --`. |
| 4 | `Create_InSectionWithoutDefinition_ShouldNotValidate` and `Create_WithNullGuidField_ShouldNotThrow` pass. |
| 5 | No `Directory.Packages.props` or csproj change. Decorator uses `DomHelper.SectionDefinitions.ReadAll()` against the inner mock. |
| 6 | Solution `dotnet test`: AssetManagement 1068 passed / 3 skipped; FacilityManagement 584; PlanAndBuild 426 / 9 skipped; InfraOpsProperties 317 / 3 skipped; Packaging 4. 0 failures. |

No seeded descriptor types disagreed with current mapper output; no seed edits were needed.

Files: `Setup\FieldValueTypeValidatingMessageHandler.cs`, `Setup\DomFieldValueTypeMismatchException.cs`, `Connections\ConnectionDomStorageProvider_FieldTypeTests.cs` (new); `ConnectionHelper.cs` (wraps the handler, adds `CreateConnection(DomSLNetMessageHandler)` overload).

## Out of scope

- Fixing the codegen template that emitted `string`.
- Enabling `validateAgainstDefinition: true` (separate follow-up; needs descriptor seeding audit).
- Sharing the decorator with other test projects.

## Links

- Prior art: `SkylineCommunications/SLC-S-InfraOps` `CommonInfraOpsSharedTests` / `AssetManagementModuleFactory`.
- Upstream mock source: `SkylineCommunications/Skyline.DataMiner.Utils.DOM` `Utils.DOM/UnitTesting/DomSLNetMessageHandler.cs`.
