# Keep domain packages independent and compose them through InfraOps.Common

Status: accepted

InfraOps is implemented as separate domain packages because each domain owns its models, repositories, validation, and local lifecycle rules. Cross-domain behavior is composed in `Skyline.DataMiner.SDM.InfraOps.Common`, the consumer-facing package, rather than by adding reverse references between domain packages. This prevents circular project dependencies, keeps domain ownership explicit, and gives consumers one entry point:

```csharp
var infraOps = InfraOpsApiComposition.Create(connection);

infraOps.AssetManagement;
infraOps.FacilityManagement;
infraOps.PlanAndBuild;
infraOps.InfraOpsProperties;
```

## Package responsibilities

| Package | Responsibility |
|---|---|
| `SDM.InfraOps.Core` | Lower-level primitives and utilities shared by domain packages. It must not reference a domain package or `SDM.InfraOps.Common`. |
| `SDM.AssetManagement.Common` | Assets, classes, ports, Connections, reservations, histories, and Asset-local validation and lifecycle rules. |
| `SDM.FacilityManagement.Common` | Facilities and their location hierarchy. |
| `SDM.PlanAndBuild.Common` | Jobs and planning data. |
| `SDM.InfraOpsProperties.Common` | Property definitions and PropertyValues. |
| `SDM.InfraOps.Common` | Consumer composition and workflows that require multiple domain packages. |

The domain projects may keep valid one-way dependencies. For example, AssetManagement references FacilityManagement because Asset placement validation reads Facility data. That does not require top-level orchestration because FacilityManagement does not reference AssetManagement.

If FacilityManagement later needs Asset data, it must not add a reverse reference to AssetManagement. The package that needs external knowledge defines a narrow interface, such as an Asset reference checker, and `SDM.InfraOps.Common` supplies the adapter. If the behavior coordinates a complete workflow across domains, the workflow belongs directly in `SDM.InfraOps.Common`.

## Dependency rules

```text
                    SDM.InfraOps.Common
                  /          |          \
       AssetManagement   PlanAndBuild   InfraOpsProperties
              |
      FacilityManagement

Each domain package -> SDM.InfraOps.Core
```

- Dependencies point downward; sibling domain packages do not reference each other in both directions.
- A domain package owns the interface for behavior it consumes.
- `SDM.InfraOps.Common` implements or connects cross-domain adapters.
- Domain-only behavior remains in its domain package.
- Cross-domain behavior is not duplicated in Automation, GQI, Protocol, native API, or TMF entry points.
- Host-specific packages obtain fully composed helpers through `SDM.InfraOps.Common`.

## Why Asset deletion is composed at the top

Deleting an Asset affects records owned by several domains:

- AssetManagement deletes owned Connections, DataPorts, and PowerPorts and detaches direct child Assets.
- PlanAndBuild removes the Asset from `AssetsUsed` and retains affected `ConnectionsOnJob` entries as removed snapshots.
- InfraOpsProperties deletes PropertyValues with exact `Scope == "Asset"` and the exact linked Asset identifier, across all SubIDs.
- Shared Property definitions are not deleted.
- Asset history deletion remains outside this cascade until its retention and storage semantics are explicitly decided.

AssetManagement defines `IAssetDeletionMiddleware` because `AssetManagementApiHelper` consumes that interface. Moving the interface into `SDM.InfraOps.Common` would force AssetManagement to reference its own upper composition package and create a cycle. `SDM.InfraOps.Common` supplies the concrete cross-domain middleware.

Direct `AssetManagementApiHelper` construction is internal because correct construction has dependencies and invariants owned by composition. Consumers use the Automation, GQI, or Protocol `GetAssetManagementApiHelper` extensions, or `InfraOpsApiComposition.Create(connection)`. PlanAndBuild reference validation uses the narrow read-only Asset repositories it needs rather than constructing an incomplete Asset helper. No incomplete Asset helper or fallback deletion middleware exists. Silent or optional cleanup is not allowed.

## Failure and recovery semantics

Asset DOM deletion, Job updates, and PropertyValues deletion use separate stores and cannot be one atomic transaction. The implementation therefore:

- validates before destructive work;
- checks Asset existence before deletion so missing-Asset deletes do not clean unrelated dependencies;
- reads remaining ports and Connections after successful Asset DOM deletion and snapshots Connection details before their cleanup;
- reports cleanup failure by Asset and stage;
- never returns success-shaped results for incomplete cleanup;
- exposes idempotent `.NET` recovery through `RecoverAssetDeletion`;
- does not add a recovery endpoint or change normal missing-Asset DELETE behavior.

The composed Asset helper retains the configured deletion middleware because recovery runs outside the normal repository middleware pipeline after the Asset may already be absent.

## Configurable Job representation

An immutable `AssetDeletionPolicy` is supplied to
`InfraOpsApiComposition.Create(connection, policy)`. It selects independently
whether existing Asset and Connection entries on Jobs are removed or retained as
removed snapshots. The default remains removal of Asset entries and retention of
Connection snapshots using cable tags. A workflow can select Asset/port display
names explicitly to preserve interactive script snapshot content.

These are representation choices, not switches for mandatory cleanup. Every
policy still removes live Connections, ports and exact Asset PropertyValues and
detaches surviving children. Recursive hierarchy deletion remains caller-owned.

The policy is constructor configuration on the existing deletion middleware;
ordinary `Assets.Delete` remains the single deletion interface. A workflow creates
a separate composition after its user has chosen, rather than mutating options
on a shared helper, using ambient state, or bypassing the repository pipeline.
The framework does not need a per-call options parameter. The accepted cost is
constructing another set of helpers for a selected workflow.

Removed Asset snapshots require name, Asset class and primary IPv4 metadata to
be captured before DOM deletion. This is a narrow exception to post-delete
dependency discovery: Connection and port cleanup dependencies are still queried
after the DOM delete. Bulk operations retain the names of all requested existing
Assets for endpoint display when both Connection endpoints are deleted.

Recoverable cleanup outcomes carry an immutable `AssetDeletionRecoveryContext`
with the original policy, Asset metadata and captured Connection details.
Snapshots survive retries even when endpoint ports no longer exist. Context-based
recovery checks the composition policy and rejects mismatches. Identifier-only
recovery is restricted to default compositions; it cannot reconstruct deleted
Asset metadata for a custom policy. Callers retain or serialize custom contexts;
there is no new persistent recovery store.

History remains outside the shared cascade. Future script migration must keep its
history preparation read-only before deletion and its history writes after
successful deletion/cleanup. The script migration is deferred; this decision does
not claim its choices already use the new composition.

## Known design debt: two-phase middleware initialization

`AssetDeletionMiddleware` is currently created and then configured:

```csharp
var cascade = new AssetDeletionMiddleware(AssetDeletionPolicy.Default);
var assetManagement = new AssetManagementApiHelper(
    connection,
    facilityManagement,
    peopleApi,
    cascade);

var planAndBuild = new PlanAndBuildApiHelper(connection);
var infraOpsProperties = new InfraOpsPropertiesApiHelper(connection);

cascade.Configure(assetManagement, planAndBuild, infraOpsProperties);
```

This ordering breaks a construction cycle: AssetManagement must receive the middleware before its Assets repository can be built, while the middleware currently needs the composed AssetManagement helper and the other domain participants. Moving the `Configure` assignments directly into the constructor is therefore not possible without first changing those dependencies.

The current implementation guards against use before configuration and against repeated configuration. It is safe because composition configures the middleware synchronously before publishing any helper, but it temporarily creates an invalid object and makes correct initialization depend on call order.

The preferred follow-up is to replace helper-level dependencies with narrow, independently constructible repository or participant adapters. The middleware could then receive every required dependency in its constructor and be valid immediately. The refactor must preserve:

- one complete composition entry point;
- middleware ordering, with validation before destructive cleanup;
- post-Asset-delete dependency reads and Connection snapshots before Connection cleanup;
- immutable Job representation policies and explicit custom-policy recovery contexts;
- per-stage failure reporting;
- idempotent recovery after the Asset DOM record is absent;
- no reverse dependency from a domain package to `SDM.InfraOps.Common`;
- no duplicated cascade behavior in host-specific packages.

## Consumer and versioning guidance

Consumers needing one complete InfraOps entry point install `Skyline.DataMiner.SDM.InfraOps.Common`. Its NuGet dependencies bring the domain packages and `SDM.InfraOps.Core` transitively. Consumers that deliberately need only one domain can continue referencing its Common package directly, subject to the fail-fast Asset deletion restriction.

The project and package are named `SDM.InfraOps.Common`, while the existing `Skyline.DataMiner.SDM.InfraOps.Orchestration` namespaces and public type names are temporarily preserved to avoid an unnecessary source-level breaking change. A future major release may simplify those namespaces as part of a deliberate compatibility change.

## Rejected alternatives

- **Put PlanAndBuild cleanup in AssetManagement:** creates a circular dependency because PlanAndBuild already references AssetManagement.
- **Move everything into one physical project now:** simplifies the graph superficially but couples every domain, forces all dependencies on every internal consumer, and makes ownership harder to maintain.
- **Let each host implement the cascade:** duplicates behavior and allows native, TMF, Automation, GQI, and Protocol paths to diverge.
- **Make cascade participants optional:** permits incomplete deletion and orphaned cross-domain data.
- **Move the deletion interface to InfraOps.Common:** reverses the dependency from its consumer to its implementation package and creates a cycle.
- **Temporarily change a shared middleware's deletion options:** leaks policy between operations or concurrent callers.
- **Add policy handling outside the cascade:** duplicates cleanup and risks running it before successful DOM deletion.
