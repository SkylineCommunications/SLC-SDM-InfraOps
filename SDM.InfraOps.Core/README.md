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
