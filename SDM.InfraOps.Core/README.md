# Skyline.DataMiner.SDM.InfraOps.Core

Shared foundation library for the SDM InfraOps NuGet packages.

Contains types that are common across all SDM domain packages:
- `ChangeTrackingBase` — base class for all domain model types
- `ValidationResult` — return type for all validation operations
- `ValidationException` / `InfraOpsException` — domain exceptions
- `InfraOpsSerialization` — JSON serialisation helpers
- `SdmFilterExtensions`, `SDMObjectReferenceExtensions`, `BulkRepositoryQueryExtensions` — consumer extension methods

By placing these in a single assembly, downstream packages that reference multiple SDM
domain packages simultaneously are free from CS0433 duplicate-type errors.

## Null entities

Validators deriving from `ValidatorBase<T>` never throw on a `null` entity:

- `Validate(null, action)` returns a failed `ValidationResult` keyed by the validator's entity-level
  field (e.g. `RackValidationField.Rack`) with the message `"<Entity> cannot be null."`
  (e.g. `"Rack cannot be null."`). Each validator supplies it via `CreateNullEntityResult()`.
- `ValidateAndThrow(null, action)` throws a `ValidationException` (not `ArgumentNullException`).
- `ValidateBulk` returns one result per input item, in the original order; `null` items get the
  null failure and only the non-null items are validated.
- Callers (e.g. scripts) should map the entity-level field to their entity selection widget.
