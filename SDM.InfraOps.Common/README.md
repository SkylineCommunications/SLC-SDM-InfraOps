# SDM.InfraOps.Common

The consumer-facing Common package for InfraOps SDM. A single `InfraOpsApiComposition.Create(connection)` call provides Asset Management, Facility Management, Plan &amp; Build, and InfraOps Properties helpers. Cross-domain operations, including Asset deletion, execute with all required participants.

```csharp
var infraOps = InfraOpsApiComposition.Create(connection);

var assets = infraOps.AssetManagement;
var facilities = infraOps.FacilityManagement;
var planAndBuild = infraOps.PlanAndBuild;
var properties = infraOps.InfraOpsProperties;
```

Domain packages remain separate internally. `SDM.InfraOps.Common` is the upper composition layer for workflows that require more than one domain and prevents circular dependencies between sibling packages.

The source repository records the package ownership rules, dependency direction, Asset deletion design, and recovery semantics in `docs/adr/0001-infraops-package-boundaries-and-composition.md`.

## Asset deletion policy

Default deletion removes Asset entries from Jobs and retains Connection entries
as removed snapshots using their cable tags. Ports, Connections and exact Asset
PropertyValues are always cleaned; surviving direct child Assets are detached,
not recursively deleted.

Choose an immutable policy when creating a separate composition for a workflow:

```csharp
using Skyline.DataMiner.SDM.AssetManagement.Deletion;
using Skyline.DataMiner.SDM.InfraOps.Orchestration;

var policy = new AssetDeletionPolicy(
    assetJobEntryMode: JobEntryRemovalMode.KeepRemovedSnapshot,
    connectionJobEntryMode: JobEntryRemovalMode.Remove,
    connectionSnapshotFormat: ConnectionSnapshotFormat.AssetAndPortNames);
var selected = InfraOpsApiComposition.Create(connection, policy);

selected.AssetManagement.Assets.Delete(asset);
// The same fixed policy applies to bulk deletion.
selected.AssetManagement.Assets.Delete(otherAssets);
```

The two Job-entry choices are independent. `KeepRemovedSnapshot` updates existing
Job entries with removal state and captured details; it does not link the deleted
Asset to new Jobs. `AssetAndPortNames` supplies human-readable Connection endpoint
snapshots instead of cable tags when Connections are retained. Other compositions
on the same connection keep their own policies.

## Recovery

If Asset DOM deletion succeeds but cleanup fails,
`AssetDeletionCascadeException.Outcomes` identifies each failed stage and supplies
its `RecoveryContext`. Keep that context (or serialize it with the caller's JSON
serializer) for an explicit retry:

```csharp
using Skyline.DataMiner.SDM.InfraOps.Orchestration.AssetDeletion;

AssetDeletionRecoveryContext pending = null;
try
{
    selected.AssetManagement.Assets.Delete(asset);
}
catch (AssetDeletionCascadeException failure)
{
    pending = failure.Outcomes
        .Single(outcome => outcome.AssetIdentifier == asset.Identifier)
        .RecoveryContext;
    // Report the failure. DOM failures have no recovery context.
    throw;
}

```

The catch example illustrates capturing information before propagating the
failure; the later recovery step belongs to the caller's retry workflow, not the
same successful fall-through. For bulk failures, retain each non-null context.
At that explicit recovery step:

```csharp
selected.RecoverAssetDeletion(pending);
```

Recovery is idempotent and rejects a different composition policy or an Asset
that still exists. If recovery fails again, retain its updated recovery context.

`RecoverAssetDeletion(identifier)` remains available only on default compositions.
Custom policies require the original context because Asset metadata no longer
exists after deletion. No persistent journal or automatic restart recovery is
provided. Recovery does not retry history handling: history remains caller-owned
and outside this cascade.

The interactive script has not yet been migrated to this interface. See
`docs/middleware-patterns.md` for middleware configuration and ordering examples.
