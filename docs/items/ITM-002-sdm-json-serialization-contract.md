# ITM-002: JSON serialization contract for SDM models

| Field | Value |
|---|---|
| Status | In review |
| Parent track | [TRK-002](../tracks/TRK-002-sdm-import-export-bulk-reads.md) |
| Spec | [SPC-002](../specs/SPC-002-sdm-json-serialization-contract.md) |
| Collaboration | [308489](https://collaboration.dataminer.services/myTasks/list/task/308489) |
| Assignee | Miguel Garrido |
| Created | 2026-10-28 |

## Problem

Import/export needs to serialize every SDM model to JSON and read it back without losing typed references. The pre-existing converters in `SDM.InfraOps.Core` sat in the wrong namespace (`SharedCommonLibrary.Converters`), only handled `SdmObjectReference<T>`, and there was no public entry point that applied the InfraOps settings with secure deserialization. `PnoObjectReference<T>` and `ISdmObjectReference<T>` fields serialized as objects (`{"Identifier":...}`) and could not be read back because the structs have no settable members.

## Current state (verified 2026-10-28)

Implemented on `SDM_Bulk_ImportExport_DCP_308489`, commits `e5ea312`, `17cd4a1`, `3a4dbfa`, `9d5c57a`, `5dec879`.

- `SDM.InfraOps.Core\Converters\InfraOpsJsonConvert.cs` (new, public static): `SerializeObject<T>`, `SerializeObject(object, Type)`, `DeserializeObject<T>` via `SecureNewtonsoftDeserialization`, `DeserializeObject(string, Type)` via `JsonConvert` with a `TypeNameHandling` guard.
- `InfraOpsContractResolver` now resolves converters for `SdmObjectReference<>`, `PnoObjectReference<>`, `ISdmObjectReference<>`; hides `SdmObject<>.Reference`; makes `SdmObject<>.Identifier` writable. Match switched from `jsonProperty.PropertyName` to `member.Name` so a naming strategy cannot break it.
- `PnoObjectReferenceConverter<T>` and `ISdmObjectReferenceConverter<T>` (new, internal). All three converters write the identifier as a JSON string and read it back through the struct's string conversion.
- `InfraOpsSerialization.DefaultSettings`: `InfraOpsContractResolver` + `StringEnumConverter`.
- `Skyline.DataMiner.Utils.SecureCoding` added as a package reference to Core, pinned 2.2.3 in `Directory.Packages.props`.
- 25 `*JsonRoundTripTests.cs` files across `SDM.AssetManagementTests`, `SDM.FacilityManagement.Tests`, `SDM.PlanAndBuild.Tests`, `SDM.InfraOpsProperties.Tests`; 129 tests, all green (`dotnet test <project> --filter FullyQualifiedName~JsonRoundTrip`).

## Deliverable

A public `InfraOpsJsonConvert` facade in `SDM.InfraOps.Core` whose output round-trips every concrete SDM model in the four `*.Common` packages, with references as plain identifier strings, enums as names, audit fields (`CreatedAt`, `CreatedBy`, `LastModified`, `LastModifiedBy`, `State`) exported but ignored on import, and `Identifier` importable.

## Acceptance criteria

1. **Round-trip.** For every concrete model in `SDM.AssetManagement.Common`, `SDM.FacilityManagement.Common`, `SDM.PlanAndBuild.Common`, `SDM.InfraOpsProperties.Common`, a test populates all reference fields, serializes, deserializes, asserts `restored.Should().Be(original)` and that re-serializing yields byte-identical JSON. **Met** (25 test files).
2. **Reference shape.** `SdmObjectReference<T>`, `PnoObjectReference<T>`, `ISdmObjectReference<T>` serialize as a JSON string holding the identifier, or `null` when empty. **Met** (`*_Serialize_WritesReferencesAsStringsAndEnumsAsNames`, `*_Serialize_EmptyReferencesAreNull`).
3. **Null / empty on read.** JSON `null` and `""` deserialize to the struct default for Pno and ISdm references. **Met** (`*_Deserialize_NullAndEmptyReferencesBecomeDefault`). For `SdmObjectReference<T>` the result holds a null `Identifier`; see review point 4.
4. **Audit fields export-only.** `CreatedAt`, `CreatedBy`, `LastModified`, `LastModifiedBy`, `State` appear in output and are default after import. **Met** (`*_AuditFields_ExportedButNotImported`).
5. **Enums by name.** All enum fields and enum lists serialize as names. **Met**.
6. **Secure deserialization.** `DeserializeObject<T>` goes through `SecureNewtonsoftDeserialization`; the untyped overload rejects any `TypeNameHandling` other than `None`. **Met**, with review point 1 open.
7. **Full suite.** `dotnet test` at solution root green. **Not yet run as part of this item**; only the filtered JSON suites were run.

## Review points (open)

1. `DeserializeObject(string, Type)` assigns `settings.SerializationBinder = null` on the shared `InfraOpsSerialization.DefaultSettings`. The value is already null, so it is a no-op today, but it mutates a public static singleton that any consumer can also mutate. Recommend making `InfraOpsSerialization` static with a private settings instance, or exposing a factory.
2. Null `json` throws `ArgumentException`; `ArgumentNullException` is the correct type.
3. `SecureCoding` bumped to 2.2.3, `SecureCoding.Analyzers` left at 2.2.1.
4. `SdmObjectReferenceConverter<T>.ReadJson` does `new SdmObjectReference<T>((string)reader.Value)` and so yields a reference with a null `Identifier` for JSON `null`, whereas the other two converters return `default`. Equality treats both as empty, so round-trips pass, but the asymmetry is untested in isolation.
5. `SerializeObject<T>(T value)` passes `typeof(T)` as the declared type; callers holding a base-typed variable get the base contract. Acceptable with `TypeNameHandling.None`; document it.

## Out of scope

- `System.Text.Json`.
- Versioning or schema evolution of the exported JSON.
- The import/export scripts that consume this contract.

## Links

- Previous converters: `SharedCommonLibrary.Converters` in SLC-S-InfraOps.
- `Skyline.DataMiner.Utils.SecureCoding` `SecureNewtonsoftDeserialization`.
