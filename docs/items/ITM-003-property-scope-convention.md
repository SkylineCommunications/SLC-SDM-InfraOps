# ITM-003: PropertyScope convention and generic Properties lookups

| Field | Value |
|---|---|
| Status | In review |
| Parent track | [TRK-002](../tracks/TRK-002-sdm-import-export-bulk-reads.md) |
| Spec | [SPC-003](../specs/SPC-003-property-scope-convention.md) |
| Collaboration | [308489](https://collaboration.dataminer.services/myTasks/list/task/308489) |
| Assignee | Miguel Garrido |
| Created | 2026-10-28 |

## Problem

InfraOps Properties are stored in a separate SDM module and are linked to an SDM object by `(LinkedObjectID, Scope, SubID)`. The scope is, by convention, the model's class name ("Asset", "Rack", ...), but nothing in the code enforced that and every caller had to pass the scope string by hand. A bulk importer/exporter iterating over heterogeneous model types has no way to derive the scope from the type.

## Current state (verified 2026-10-28)

Implemented on `SDM_Bulk_ImportExport_DCP_308489`, commits `a68fd86`, `bc7ce9f`, `a98cffd`.

- `SDM.InfraOps.Core\Models\PropertyScopeAttribute.cs` (new, public sealed): class-level, not inherited, rejects blank scope; `GetScope<T>()` / `GetScope(Type)` with a `ConcurrentDictionary<Type,string>` cache; throws `InvalidOperationException` naming the type when the attribute is missing.
- `[PropertyScope("<ClassName>")]` applied to all 23 concrete models: Asset, AssetClass, AssetManagerAppSettings, CableType, Connection, DataPort, DeviceType, History, InfraopsReservation, PortType, PowerPort, Desk, Facility, FacilityManagerAppSettings, Floor, Rack, Room, Row, Site, Zone, JobType, PlanAndBuildAppSettings, PlanAndBuildJob.
- `SDM.InfraOpsProperties.Common\Extensions\PropertyRepositoryExtensions.cs`: `GetByScope<T>()`, `GetByScopeAndName<T>(name)` delegating to the string overloads.
- `SDM.InfraOpsProperties.Common\Extensions\PropertyValuesRepositoryExtensions.cs`: `GetByLinkedObjectID<T>(T, subId)`, `GetSingleOrDefaultByLinkedObjectID<T>(T, subId)`, `CopyPropertyValues<T>(T source, T target, ...)`.
- `SDM.InfraOps.PackagingTests\PropertyScopeConventionTests.cs`: every concrete `SdmObjectBase<>` descendant in the four Common assemblies (except `Property`, `PropertyValues`) must carry the attribute with scope equal to its class name; scopes must be unique case-insensitively. 2 tests, green.
- `SDM.InfraOpsProperties.Tests\Extensions\PropertyScopeGenericExtensionsTests.cs`: attribute behaviour and generic overloads against the in-memory DOM mock. Green (part of the 33-test filtered run).

## Deliverable

A type-driven scope lookup so that `properties.GetByScope<Asset>()` and `propertyValues.GetByLinkedObjectID(asset)` need no scope literal, enforced repository-wide by a convention test.

## Acceptance criteria

1. **Attribute present on every model.** `PropertyScopeConventionTests.EverySdmModelShouldDeclarePropertyScopeEqualToItsClassName` passes. **Met**.
2. **Scopes unique.** `PropertyScopesShouldBeUnique` passes. **Met**.
3. **Generic overloads equal string overloads.** For a scoped type, `GetByScope<T>()` returns the same set as `GetByScope("<ClassName>")`; same for `GetByScopeAndName<T>`, `GetByLinkedObjectID<T>`, `CopyPropertyValues<T>`. **Met** (`PropertyScopeGenericExtensionsTests`).
4. **Unscoped type fails fast.** Calling any generic overload with a type lacking the attribute throws `InvalidOperationException` whose message contains the type name. **Met**.
5. **Blank scope rejected at declaration.** `new PropertyScopeAttribute(null|""|" ")` throws `ArgumentException`. **Met**.
6. **SubID semantics unchanged.** `null` = no SubID set, `"*"` = any, other = exact. Documented in XML docs and preserved by the generic overloads. **Met** (`GetPropertyValues_WithSubIdAndWildcard_ShouldFollowSubIdSemantics`).
7. **Full suite.** `dotnet test` at solution root green. **Not yet run as part of this item**.

## Review points (open)

1. Scope values are string literals duplicated from the class name. `nameof(Asset)` would make the attribute self-maintaining on rename; the convention test already catches drift, so this is cosmetic.
2. `Property` and `PropertyValues` are excluded from the convention by a hard-coded list in the test. If a third storage-only model appears it must be added there; consider an opt-out attribute instead.
3. `GetByScope<T>` constrains `T : SdmObject<T>`, but the convention test scans `SdmObjectBase<>`. Any model deriving from `SdmObjectBase<>` but not `SdmObject<>` would carry the attribute yet be unusable with the generic overloads. None exist today.

## Out of scope

- Moving scope ownership into the generated mappers or codegen.
- Changing the stored scope values in existing DOM data.

## Links

- Legacy: `PropertyDefinitionHandler` / `PropertyValuesDefinitionHandler` in SLC-S-InfraOps (string-scope API these extensions mirror).
