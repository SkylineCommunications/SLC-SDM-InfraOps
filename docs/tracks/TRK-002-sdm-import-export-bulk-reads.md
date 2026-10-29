# TRK-002: SDM import/export bulk reads

| Field | Value |
|---|---|
| Status | Active |
| Owner | Miguel Garrido |
| Collaboration | [308489](https://collaboration.dataminer.services/myTasks/list/task/308489) |
| Branch | `SDM_Bulk_ImportExport_DCP_308489` |
| Created | 2026-10-28 |

## Goal

Give the InfraOps import/export flow everything it needs from the SDM packages to work on whole collections instead of one instance at a time: a stable JSON contract for every SDM model, and scope-based Property/PropertyValues lookups that can be resolved from the model type alone.

## Why

The task description asks for the import/export logic to stop doing per-item single SDM reads. Before that can happen the SDM packages have to offer:

1. A serialization contract that round-trips every model, including `SdmObjectReference<T>`, `PnoObjectReference<T>` and `ISdmObjectReference<T>` fields, through `Newtonsoft.Json` with secure deserialization settings. The previous converters lived under the `SharedCommonLibrary.Converters` namespace and only handled `SdmObjectReference<T>`; the review comment on the task ("missing json") refers to this gap.
2. A way to find the Properties scope of an SDM object from its type, so a bulk importer/exporter can fetch Properties and PropertyValues per model type with a single filtered read rather than hard-coding scope strings per call site.

## Scope

- `SDM.InfraOps.Core` serialization: `InfraOpsJsonConvert`, `InfraOpsSerialization`, `InfraOpsContractResolver`, the three reference converters.
- `SDM.InfraOps.Core` `PropertyScopeAttribute` and its application to every concrete SDM model in the four `*.Common` packages.
- `SDM.InfraOpsProperties.Common` generic `GetByScope<T>`, `GetByScopeAndName<T>`, `GetByLinkedObjectID<T>`, `GetSingleOrDefaultByLinkedObjectID<T>`, `CopyPropertyValues<T>` extensions.
- Round-trip and convention tests in all five test projects.

## Out of scope

- The import/export scripts themselves (not in this repository). This track supplies the contract they consume.
- Changes merged from `UnitTestFromStoSDM` (validators, change tracking, mock fidelity). See TRK-001 for the mock side.
- A `System.Text.Json` contract.

## Scope note (flagged, not fixed)

The Collaboration task's acceptance criteria describe the consumer-side change ("import/export logic no longer performs per-item single SDM reads"). None of the commits on this branch touch import/export logic, so the task's criteria cannot be closed from this repository alone. Either the task scope should be split (SDM contract here, consumer change in the import/export repo) or the task description should be amended. Decision pending.

## Work-items

| ID | Title | Status |
|---|---|---|
| [ITM-002](../items/ITM-002-sdm-json-serialization-contract.md) | JSON serialization contract for SDM models | In review |
| [ITM-003](../items/ITM-003-property-scope-convention.md) | PropertyScope convention and generic Properties lookups | In review |

## Candidate follow-ups (not yet items)

- Freeze `InfraOpsSerialization.DefaultSettings` (public mutable static; `DeserializeObject(string, Type)` mutates it). Raised in ITM-002.
- Align `Skyline.DataMiner.Utils.SecureCoding.Analyzers` with the `SecureCoding` 2.2.3 bump.
- Canonical doc `/docs/canon/` describing the JSON contract (which fields export-only, how references serialize). Proposed in SPC-002, needs approval.
- Consumer-side item in the import/export repository that replaces single reads with `Read(filter)` / `ReadPaged` and uses `GetByScope<T>()`.
