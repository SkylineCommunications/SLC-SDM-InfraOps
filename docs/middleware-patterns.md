# Middleware patterns in InfraOps

The SDM framework wraps repositories with `WithMiddleware`. Its `next` delegate
continues the existing chain; it is not another public repository invocation.
Calling the wrapped repository again for the same operation can recurse or run
the cascade twice.

The supplied framework guide describes the patterns below. Use the installed
interfaces and generated wrappers as the exact contract: example guides may show
different bulk signatures or omit newer operations.

## Validation before an operation

`ValidationMiddleware<T>` in InfraOps.Core checks a validator before calling
`next`. AssetManagement specializes it as `AssetValidationMiddleware`. Invalid
Assets must fail before either DOM deletion or dependency cleanup occurs.

Conceptual shape:

```csharp
public void OnDelete(Asset asset, Action<Asset> next)
{
    ValidateDelete(asset); // Throw the repository's validation exception on failure.
    next(asset);
}
```

This snippet illustrates ordering, not a second validator implementation.

## Post-delete cascade

The Asset deletion middleware calls `next` first, then cleans dependencies for
successfully deleted Assets. It also captures metadata that cannot be recovered
after DOM deletion, reports partial failures, and supports explicit recovery.
Bulk DOM failure is not assumed to mean every Asset survived: the middleware
discovers the actual per-Asset outcome before cleanup.

Single and bulk operations use the normal repository methods:

```csharp
var composition = InfraOpsApiComposition.Create(connection);
composition.AssetManagement.Assets.Delete(asset);
composition.AssetManagement.Assets.Delete(assets);
```

There is no transaction spanning DOM, Jobs and PropertyValues. A failure after
`next` requires explicit recovery, not blindly repeating the original delete.

## Constructor-configured middleware

`AssetDeletionMiddleware` receives immutable policy in its constructor.
Composition supplies its domain participants before publishing the helper.

```csharp
var policy = new AssetDeletionPolicy(
    JobEntryRemovalMode.KeepRemovedSnapshot,
    JobEntryRemovalMode.Remove);
var composition = InfraOpsApiComposition.Create(connection, policy);
composition.AssetManagement.Assets.Delete(asset);
```

Do not temporarily set a shared policy, introduce thread-local options, or encode
control flags in Asset DOM fields. The existing two-phase participant
configuration is separate documented design debt; policy itself never changes.

## Layer ordering

The actual Asset helper wiring is:

```csharp
Assets = assetRepository
    .WithMiddleware(assetDeletionMiddleware)
    .WithMiddleware(new AssetValidationMiddleware(assetValidator))
    .WithMiddleware(new IdentifierMiddleware<Asset>());
```

Each call wraps the previous layer. Execution enters Identifier, Validation,
Deletion, then the DOM repository. On return, the Deletion layer performs cleanup.
Adding another cascade around `Assets` would not replace the existing one.

## Logging, timing and error handling

For a middleware specializing deletion, the basic timing/error pattern is:

```csharp
public void OnDelete(Asset asset, Action<Asset> next)
{
    var stopwatch = Stopwatch.StartNew();
    try
    {
        next(asset);
    }
    finally
    {
        stopwatch.Stop();
        RecordDuration(asset.Identifier, stopwatch.Elapsed);
    }
}
```

This is an illustrative method body; `RecordDuration` stands for a host-supplied
reporting dependency, not an existing InfraOps method. Its constructor should
receive that dependency. Reporting must not mask the operation's original error.
If a logging middleware catches an error, report and rethrow it; do not convert
incomplete cleanup into success. Place logging/timing outside validation to
observe rejected operations as well.

Implement only the supported operation interfaces needed where the generated
wrapper accepts them. The existing Asset deletion pass-through base supports the
full bulk repository contract; this change does not rewrite that framework seam.

## Explicit cleanup recovery

For a cleanup failure, retain each non-null
`AssetDeletionCascadeException.Outcomes[].RecoveryContext`. Recreate a composition
with the same policy if needed, and invoke:

```csharp
composition.RecoverAssetDeletion(context);
```

The context may be serialized by the caller. A DOM failure has no recovery context
because cleanup must not run while the Asset still exists. Repeated recovery must
not duplicate removed Job snapshots or modify unrelated entries. If a retry
fails, retain the updated context from that failure.

Default-policy identifier-only recovery remains:

```csharp
composition.RecoverAssetDeletion(assetIdentifier);
```

Custom policies reject this shorter form: the identifier does not contain the
deleted Asset's display metadata or the user's original choices.
