# Legacy → SDM Model Conversion Playbook

## 0. Golden rules (read first)

1. **`DomIds.cs` (`SharedMappers\DomIds.cs`) is the single source of truth.** Never trust literal GUIDs sitting in a `.g.cs` file. Always wire IDs to `SharedMappers.DomIds.<Module>.{Definitions,Sections,Enums,Behaviors}.*`.
2. **`.g.cs` files in this repo are not live-regenerated.** `[GenerateExposers]`/`[SdmDomStorage]`/`[SdmDomMapper]` attributes are commented out on model classes across every module, and the real `Skyline.DataMiner.SDM.SourceGenerator` package is not wired into any `.csproj`'s build. These files were generated once (or hand-authored to mimic generator output) and are now hand-maintained source. Treat them as regular code to edit carefully — but preserve their existing shape/conventions exactly, since other tooling and mental models assume that shape.
3. **Never commit without a fresh, explicit go-ahead for that specific diff.** Implementing a change is not permission to commit it — treat every commit as its own approval gate.
4. **Never push without showing the exact refspec and getting explicit confirmation first.** Orgs with SAML SSO enforcement can intermittently invalidate a cached git token, causing a 403 on push that is unrelated to the code — that's an auth/browser fix, not a code bug.
5. **Legacy is inconsistent by design, not by convention.** Never assume uniform behavior across fields, modules, or subsystems — verify per-field against the actual legacy code every time, even when a sibling field "obviously" should behave the same way.

---

## 1. Discovery phase

**Do:**
- Read the real legacy file first: `Wrappers\{Entity}Wrapper.cs` (behavior/convenience API), `Sections\*.cs` (field mapping + `ApplyChanges()`), `Validation\{Entity}ValidationHandler.cs` (business rules), `Handlers\{Entity}DefinitionHandler.cs` (DOM query/CRUD, and note the base class it extends).
- Check the **base class** the legacy handler extends. A handler extending a "unique name" style base class (vs. a plain name/definition handler) signals that uniqueness enforcement is expected — this is often the only visible clue that a uniqueness constraint exists at all, since the actual enforcement can be missing or only implied by `.SingleOrDefault()`-style query usage.
- Grep legacy for **UI-only business rules** — Automation scripts, IAS dialog presenters, and similar UI-layer code sometimes contain validation that never made it into the DOM-layer handler/wrapper. These rules still need to be ported into the SDM validator/middleware layer even though they're "hidden" outside the obvious validation files.
- Check whether a field is genuinely **soft-deleted** in legacy (`[Obsolete]`, get-only, excluded from `ApplyChanges()`/persistence). If so, the new field must mirror both the obsolete marker and the write-block — not just one or the other.

**Don't:**
- Don't assume one module's convention applies to another. Different modules (and even different subsystems within the same module) can have different original authors and different behavioral conventions — e.g., one subsystem's collection-mutation methods might throw on duplicate/not-found while another's silently no-ops.
- Don't assume a field name maps 1:1 in meaning across modules. Verify by reading the actual resolver/lookup code, not by name similarity (a field literally named "AssignmentGroup" resolved to a Team type, not an Organization type, in one case — only confirmed by reading the resolution code).

**Lookout:**
- A DOM Definition ID (or similar constant) recorded in `DomIds.cs` can be stale relative to what a specific mapper file actually uses at runtime. If seed/test data behaves oddly against a definition ID, cross-check the mapper's own constant directly rather than trusting `DomIds.cs` alone.

---

## 2. Enum serialization audit (do this explicitly, per field — never assume)

Legacy enum serialization is a genuine **mix** of string-backed and int-backed enums. There is no repo-wide rule, and assuming one is the single most common source of silent data-corruption bugs in this kind of port.

- **How to tell:** in `DomIds.cs`, does the enum's containing static class expose `ToEnum`/`ToValue` helper methods? → it is **string**-backed (a Discreet value). Is it a bare `enum` declaration with no helper class? → it is **int**-backed (a raw ordinal).
- Cross-verify against the legacy Section's field-mapping/`ApplyChanges()` code: `Convert.ToString(value)` + `.ToEnum(...)` indicates string storage; `Convert.ToInt32/64(value)` + `(EnumType)cast` indicates int storage.
- A repository doing `GetValue<int>`/`(int)cast` for a field that is actually string-backed in legacy is a live, silent bug — it will misread/miswrite real DOM data even though the code compiles and looks correct.
- The fix per field has three parts: deserialize (`GetValue<string>` + the matching `ToEnum` helper), serialize (`AddOrUpdateValue<string>` + `ToValue`), and the filter switch-case (string comparison instead of int comparison). Also check any OrderBy logic for that field — switching storage type from int to string can silently change sort ordering (alphabetical vs. ordinal).
- Never fix one occurrence of a shared enum type and assume sibling usages elsewhere are automatically consistent — the same enum type can be serialized differently in different sections/models if those sections were authored independently. Check each usage site individually.

---

## 3. Model authoring (`Models/*.cs`)

**Section-shape rule** — this is what the (theoretical/original) code generator infers from the C# shape, not from any attribute, and this shape convention should still be followed even when hand-editing:

| C# shape | DOM result |
|---|---|
| flat scalar directly on the entity | flat field in the main/base DOM section (one flat exception per Definition) |
| single-instance child class property (`ChangeTrackingBase`-derived, own FieldHandler) | single DOM section |
| `List<T>` of a plain sealed `IEquatable<T>` POCO (not `ChangeTrackingBase`-derived) | repeating DOM section |

**Do:**
- Keep models as **pure POCOs / change-tracking plumbing only**: properties, `ChangeTrackingField`s, `Changed`, `IsNew`, `ResetChangeTracking()`. No business logic, no computed/convenience methods, no service calls on the model itself — those belong in a dedicated extensions class (see §9).
- Match legacy's mutability contract exactly: if legacy makes a field get-only/soft-deleted, mark it `[Obsolete("...")]` here too, and restrict its setter (`internal`, or fully immutable if legacy blocks even internal writes — verify which).
- Cross-module references (e.g. a property typed as a reference to a type defined in a sibling module's project) only work when the target type lives in a project-referenced sibling project within the same solution — this relies on compile-time syntax-tree visibility, not any runtime mechanism.

**Don't:**
- Be cautious wrapping a raw value-type list (e.g. `List<Guid>`) directly if a live code generator is ever reintroduced — some generators do not support raw value-type list elements as a supported list shape, and would silently skip the property from generation. In a repo where `.g.cs` files are hand-maintained (not live-generated), this can be worked around by hand-editing the repository file to match — but this is fragile and should be flagged clearly as a latent risk if a real generator run is ever reintroduced.
- Never give an `SdmObjectReference<T>`-typed property to a type that comes from an already-compiled external package (e.g. a NuGet-shipped type). This kind of reference type is synthesized per-compiling-project by scanning the *current project's own syntax trees* — an already-compiled type can never qualify. Use a plain `Guid?` instead, and validate existence by querying the external API's own repository (`.Read(guid) != null`).

**Lookout — struct gotcha:**
- `SdmObjectReference<T>` is a **struct**, not a class. Using the `?.` null-conditional operator on a reference-type receiver to reach a struct-returning member (e.g. `entity?.SomeReference`) silently wraps the result in `Nullable<SdmObjectReference<T>>`, which evaluates `HasValue == true` for any non-null receiver — completely bypassing the struct's own custom null-equality operator. **Never use `?.` for a custom-null check on a struct-typed member** — use an explicit `x != null && x.Member != null` guard instead. This class of bug is easy to introduce silently during an extension-method extraction/refactor and is only caught by a dedicated test.
- A true `Guid?` does not have this problem — `?.HasValue` flattens correctly since `Nullable<Nullable<T>>` collapses to `Nullable<T>`.

---

## 4. Mapper wiring (`Mappers/*DomMapper.g.cs`)

**Do:**
- Every ID — `DomDefinitionId`, every `SectionDefinitionID`, every `FieldDescriptorID` — must be a direct reference to `SharedMappers.DomIds.<Module>.{Definitions,Sections}.*`. Zero literal GUIDs, ever.
- If the model's property name differs cosmetically from the DomIds field name (casing, spelling), alias at the reference site — don't rename the DomIds constant, and don't rename an already-approved public model property either.
- Before swapping a literal GUID for a DomIds reference, grep the whole repo (including any XML/protocol configuration) to confirm nothing else depends on the old literal value. If live/persisted DOM data may already use the old GUID, flag this explicitly — that's a live-system concern outside the code repo's scope, not something to silently work around.

**Don't:**
- Don't assume DOM Repository `.g.cs` files need the same GUID-reference treatment — they reference Mapper/Exposer classes by name, never raw GUIDs directly, so they become correct automatically once the Mapper is fixed.

**Lookout:**
- `DomIds.cs` can appear to have "mysteriously" changed (a huge raw diff that collapses to a tiny real diff once whitespace is ignored) if a new section/field entry was hand-added there while wiring a new model field. Always check a whitespace-insensitive diff before assuming an unexplained external mutation occurred.

---

## 5. DOM Repository (`.g.cs`) authoring

**Do — recurring bug class to check on every repository file touched:**
- `FromInstance()` must set `IsNewInternal = false` (in the object initializer) **and** call `obj.ResetChangeTracking()` before returning. Missing either one breaks `IsNew`/`Changed` round-trip semantics silently for anything read back from storage. This is the single highest-value two-line check to run on any repository file being touched or reviewed — it has been found broken across multiple generated repository files at once, suggesting it's an easy detail to omit when hand-authoring or copy-adapting one of these files.
- For **behavior-status fields** (a `State`/`Status`-style property backed by the DOM engine's own `instance.StatusId`, not an ordinary field): `FromInstance` should read `State = X_Behavior.Statuses.ToEnum(instance.StatusId)`; `ToInstance` should set `instance.StatusId = X_Behavior.Statuses.ToValue(obj.State)` **unconditionally on every call**, not gated by an "is this a new object" check. A gate that only sets `StatusId` on create looks correct by analogy to a sibling module's pattern, but breaks any subsequent `Update()` call — because a plain update can trigger a full-object replace at the storage layer with no field-level merge, silently wiping the real status to blank. If the model's `State` property is always correctly populated from the engine on every read (via `FromInstance`), it's safe to write it back unconditionally on every write.
- Never give a behavior-status field its own exposer, DOM section field, or `FieldDescriptorID` — it must route through `instance.StatusId` exclusively, not through an ordinary section field. Treating it as an ordinary field is a common root cause of state getting silently reset or desynced from the engine's real status.

**Don't:**
- Don't call the repository's own plain `Update(entity)` method from inside hand-written combo-operation methods (e.g. a transition method that also updates fields) expecting it to route through a validation-middleware decorator — it won't. A same-class instance method call to a non-virtual method always binds at compile time to that class's own raw implementation, bypassing any decorator/middleware wrapper regardless of what type callers see the repository as at runtime. This is a real, exploitable validation-bypass bug class. **Fix pattern:** inject the validator directly into the raw repository class (an internal settable `Validator` property, wired from the owning API-helper's constructor) and call `Validator?.ValidateAndThrow(entity)` explicitly at the very top of any combo-operation method, before any mutation happens.

**Lookout — enum sites:** every enum field touched by a repository needs the §2 audit applied individually; don't fix one occurrence and assume sibling fields of the same enum type elsewhere are automatically consistent.

---

## 6. Exposers

**Do:**
- Nullable enum exposers (`Exposer<T, TEnum?>`) can't use `.Equal()` — the generic constraint requires `IEquatable<F>`, which `Nullable<T>` doesn't satisfy. Use `.UncheckedEqual()`/`.UncheckedNotEqual()` instead.
- `.Equal()`/`.Contains()`/`.NotEqual()` extension methods live in a specific SLDataGateway namespace — they require an explicit `using` in every file that writes filters, even when other related namespaces are already imported.

**Lookout:**
- A `Comparer` type used by filter-construction helpers can resolve ambiguously if a generic collections namespace is also imported in the same file (both namespaces can expose a type with that name). Fully qualify if there's any doubt about which one is being resolved.

**Lookout — XML doc `cref` resolution is stricter than C# member lookup:**
- A `cref` must name the interface that actually **declares** the member, not just any interface in the hierarchy that inherits it. Unlike a normal C# call site (where `repo.Count(filter)` resolves fine through any number of `interface X : Y, Z` "implements" hops), `<see cref="IBulkRepository{T}.Count"/>` fails with **CS1574 (could not be resolved)** if `Count` is actually declared several levels up the interface chain (e.g. on `ICountableRepository<T>`, with `IRepository<T>`/`IBulkRepository<T>` merely extending it). Fix by pointing the `cref` at the interface that truly declares the member: `<see cref="ICountableRepository{T}.Count(FilterElement{T})"/>`.
- When the member has multiple overloads (e.g. `Count(FilterElement<T>)` and `Count(IQuery<T>)`, or a project's own method with a single-item and a batch overload), an unqualified `<see cref="MethodName"/>` triggers **CS0419 (ambiguous reference)**. Disambiguate by adding the explicit parameter list: `<see cref="MethodName(Guid, string, string, string)"/>`.
- A `cref` segment prefix must be a real namespace or type nested under the current file's namespace (or something reachable via an existing `using`) — writing `<see cref="Extensions.SomeClass.SomeMethod"/>` when `Extensions` isn't actually a child namespace of the current file's own namespace fails silently as CS1574 even though a `using Full.Namespace.Extensions;` is present at the top of the file. Just use the bare class name (`SomeClass.SomeMethod`) and rely on the `using`.
- **Diagnostic technique** when the correct declaring type/overload isn't obvious from source (e.g. a member comes from a shipped NuGet package with no local source): disassemble the specific dependency DLL with `ildasm.exe <dll> /out:dump.il /text` and grep the dump for `.class interface ... TypeName\`1` to see the exact `implements` chain and member signatures. This is far more reliable than trying to `Add-Type`/reflect the assembly directly in PowerShell, which frequently fails with missing-transitive-dependency errors (e.g. `Google.Protobuf` not found) for multi-dependency SDK packages, and `Assembly.ReflectionOnlyLoadFrom` isn't supported at all on .NET (Core) PowerShell.

---

## 7. Validation / Middleware

**Do:**
- All validators should take the module's public `I*ApiHelper` interface in their constructor — never a raw connection or raw generic repository interface — and only dereference the helper's repository properties inside `Validate()`/`ValidateBulk()`, never during construction. The owning API-helper's constructor can safely pass itself (`this`) into each validator **before** its own repository properties are assigned — a deliberate "circular self-reference" that is safe only because of the above invariant. Document this pattern with an XML doc comment every time it's used; it's non-obvious and easy to break by accident in future edits.
- Use a consistent **3-phase bulk-validation pattern**:
  1. Phase 1 — no-DB business rules, fail fast.
  2. Phase 2 — in-memory batch conflict detection (checking for conflicts among items in the same batch being validated together, before any of them touch storage), fail fast.
  3. Phase 3 — DB/storage-backed uniqueness checks plus any remaining per-item checks.
  Return results as an index-aligned result list. For a natural key made of multiple fields, group using a single delimiter-joined composite string key (e.g. using an uncommon separator character, uppercased for case-insensitivity) rather than a dynamic-typed custom equality comparer — simpler and avoids dynamic dispatch overhead/fragility.
- Legacy's uniqueness pattern is frequently a **two-check** design: an in-memory check against other entries in the same batch, plus a separate storage query excluding the entries currently being saved (since they may already exist as "old" versions of themselves). A validator that implements only the storage-query half is a real, silent gap even if it looks complete at first glance — always check for the missing batch-conflict half explicitly.
- For an optional component of a composite uniqueness key (e.g. a nullable "sub-identifier" that can be null, a wildcard, or a specific value), mirror legacy's exact null/wildcard/exact-value semantics rather than inventing new behavior. Check whether the generated repository's own exposer already auto-translates a null-equality filter into the correct underlying storage filter before hand-rolling in-memory null filtering.
- For "built in, opt-out" cascade-delete behavior: implement it inside the entity's `*ValidationMiddleware.OnDelete` (both single and bulk overloads), gated by a constructor-time boolean flag threaded from the owning API-helper. If a standalone opt-in cascade-delete extension method already exists, remove it (and its dedicated tests) once the built-in version supersedes it — don't leave a dead parallel code path.
- For a cascade-delete/update that touches every value referencing a deleted/renamed entity, collect the full list of affected entities first and issue **one bulk update/delete call** at the end, instead of updating/deleting one-by-one inside the collection loop — mirrors the batched-query principle below and avoids one DB round-trip per affected row.

**Batched DB lookups for bulk validation (avoiding N+1 query loops):**
- Central pattern: a single shared generic extension, `ReadByBigOrFilter<T, TKey>(this IBulkRepository<T> repo, IEnumerable<TKey> keys, Func<TKey, FilterElement<T>> filterProvider)`, built on `Skyline.DataMiner.Net.Tools.RetrieveBigOrFilter<T, TKey>` (from the `Skyline.DataMiner.Net` NuGet package). Put this **once** in the shared validation library so every module reuses the same battle-tested big-OR-batching logic instead of hand-rolling it per module.
- Constraint gotcha: constrain the shared extension with `where T : class`, **not** `where T : SdmObject<T>`. Externally-defined types used through this same repository shape (e.g. `Person`/`Team` from an external People & Organizations package, whose base type is `ApiObject`, not `SdmObject<T>`) still satisfy `IBulkRepository<T>` through its own interface-inheritance chain, so the looser `class` constraint is what makes the helper reusable across both SDM-native and externally-packaged repositories.
- Delegate-shadowing gotcha: `Skyline.DataMiner.Net.Tools` declares its own nested `Tools.Func<T1,T2>` delegate type, which shadows `System.Func` inside that class's method signatures. Passing a `System.Func`-typed parameter straight into `Tools.RetrieveBigOrFilter` fails with a delegate-type-mismatch compile error even though the signatures look identical. Fix: wrap the parameter in a fresh lambda at the call site (`key => filterProvider(key)`) so the compiler target-types the conversion correctly, and explicitly specify the `<T, TKey>` generic arguments on the call (plain type inference fails here).
- Per-module wiring: don't call the shared generic extension directly from a validator. Add a small, typed wrapper method (e.g. `GetByScopeAndNames(IEnumerable<(string,string)>)`, `GetByJobNames(IEnumerable<string>)`) on the module's own hand-written partial DOM repository class, following the exact same `[AllowSdmMiddleware] public interface IXxxRepository : IBulkRepository<T> { ... }` + `internal partial class XxxDomRepository : IXxxRepository` pattern already used for other hand-written repository extensions (see §5/§8) — then retype the owning API-helper's exposed property from the generic `IBulkRepository<T>` to this richer interface. This composes correctly through the existing middleware-wrapping chain (confirmed empirically against existing passing tests) without needing to reverse-engineer the middleware/decorator generation mechanism.
- Validator wiring convention: keep the existing single-item validation method + its single-item DB-check helper completely untouched (still used by the `Validate`/`ValidateAndThrow` single-item pipeline). Add a **new overload** of both the validation method and its check helper that accepts a pre-built `Dictionary<TKey, List<T>>` or `ILookup<TKey, T>`, and only wire that overload into `ValidateBulk`'s Phase 3 — Phase 3 should begin with **one** batched pre-fetch call (building the lookup for the whole incoming batch) before the per-item loop, instead of one query per item inside the loop. This keeps single-item validation perf/behavior identical while batching the bulk path.
- This same batching principle extends past DOM repositories to any repository shape satisfying `IBulkRepository<T>` — including externally-packaged repositories like People & Organizations' `IPeopleAndOrganizationsApi.People`/`.Teams` — via simple extension methods (e.g. `GetExistingPersonIds(IEnumerable<Guid>)` returning a `HashSet<Guid>`) built the same way, letting a per-item "does this Person/Team exist" existence check become one batched query per validated collection instead of one remote call per reference.

**Don't:**
- Don't assume a no-op validation hook in legacy (a wrapper method that unconditionally returns an empty/passing result) means "nothing to port." It can genuinely mean legacy had zero validation there, and any new checks added are new behavior, not a ported feature — state this distinction clearly when reporting back, rather than implying parity that doesn't exist.
- Don't silently work around an unrelated build error that surfaces mid-task by commenting out attributes or otherwise touching out-of-scope code — even if that exact pattern already exists elsewhere in the codebase. Stop, report the unrelated error clearly, and ask how to proceed before touching anything outside the requested scope. Concrete example hit in practice: a sibling module's model class had a genuine pre-existing compile error (a field typed `IChangeTrackingField<Guid?>` but backed by a non-nullable `Guid` factory/property — `CS0266`), discovered only because the module under active work has a project reference to it. The fix was small and clearly scoped, but it was still paused and escalated for explicit sign-off before touching that out-of-scope file, rather than "just fixing it" because it looked trivial and unrelated.

**Lookout — behavioral convention differs by subsystem:**
- Different subsystems within the same codebase can have genuinely different conventions for the same kind of operation (e.g. one subsystem's Add/Remove convenience methods throw on duplicate/not-found, while another subsystem's silently no-ops on the same cases) — this reflects different original authors/eras, not a bug in either. Never assume one subsystem's convention for a new subsystem being ported; explicitly decide (and get sign-off) on whether to match the new subsystem's own legacy behavior or standardize on another subsystem's convention for internal consistency.

---

## 8. Helper (`*ApiHelper`) wiring

**Do:**
- Treat the public `I*ApiHelper` facade as the only externally reachable surface — underlying DOM repository classes are typically `internal`. If cross-module access is needed (one module needing to read another module's entities), **extend the target module's public helper interface** to expose the missing repository property rather than trying to reach an internal class directly. While doing this, check whether the target module's public facade is already fully complete — a facade that exposes only some of its own module's repository types (leaving others internal-only and unreachable even from within the same solution) is a real, silent gap worth flagging even when it's not the primary task at hand.
- When wiring an externally-packaged API (a NuGet-shipped API surface) into a helper: obtain it via its own dedicated factory/extension method, decide whether to expose it as a public property or keep it scoped to validator construction only, and thread it into any validator that needs existence checks — following the same capture-by-reference safety rule as §7.
- When adding a new mandatory dependency that existing tests implicitly assume doesn't exist yet (e.g. a required singleton settings object needed by a new ID-allocation feature): do not globally auto-seed it in a shared test base class if some existing tests assert exact counts around that same entity type — seed it surgically, only in the specific test files that actually exercise the new dependent pipeline.

**Don't:**
- Don't assume repository properties must be typed as the generic bulk-repository interface. Entity-specific repository interfaces (extending the generic one) can compile cleanly even through middleware-wrapping chains, via generic overloads that may be shipped inside a compiled package rather than visible via source search in the repo. Don't burn excessive time trying to fully prove the exact compiler resolution mechanism if it demonstrably compiles and behaves correctly — verify empirically and move on unless it actually causes a real failure.

---

## 9. Extensions / convenience methods

**Do:**
- Put all business-logic convenience/computed methods in a dedicated extensions file, never on the model class itself.
- **Placement rule:** an extension method belongs on the type that owns the field(s) it actually reads or writes — not reflexively on the root aggregate type. Root-level checks belong on the root type; checks specific to a child object's own fields belong on that child type, even if this means a call site now goes through the child object where legacy had a single flat API on one wrapper class. This is generally an architectural improvement worth keeping, not a regression — avoid adding a redundant root-level passthrough purely for legacy call-site parity.
- For batch/typed resolution across DOM types (resolving a heterogeneous collection of IDs that could each point to one of several possible target types): use an OR-filter with per-candidate-type identifier-equality filters, issuing one read per candidate type. This is the established idiom for this kind of polymorphic lookup.
- Name methods that perform live DOM I/O (a query) so the name reflects that it does I/O (e.g. a "Resolve"-style name rather than a plain "Get"-style name), to set the right expectation about cost/side-effects at the call site.

**Don't — collection-keying trap:**
- Never key a dictionary by a raw model instance when the model type's equality/hash-code are Identifier-based (common for DOM-backed model base classes). Two distinct unsaved instances that both have an unset/empty Identifier will collide as the same dictionary key, silently overwriting one entry's data with another's. Use an order-preserving list of key-value pairs (with reference-equality lookup if needed) instead of a dictionary keyed by model instances.

---

## 10. State machine / behavior transitions (only relevant for entities with a status/workflow)

**Do:**
- Implement an internal static state-machine class holding a dictionary keyed by (from-status, to-status) mapping to the transition path, with an "is this transition allowed" check and a "get the transition path" accessor that returns a defensive copy rather than the internal list itself.
- Implement repository-level combo methods (transition-only, update-then-transition, transition-then-update) that loop over the transition path, calling the DOM engine's status-transition API once per hop, and update the model's status property from the final resulting engine status afterward.
- When testing against a mocked DOM engine: such mocks typically do not auto-apply an "initial status" on instance creation the way a real running system would — status must be seeded explicitly, ideally by making sure the write path (`ToInstance`) always sets the engine-level status field unconditionally from the model's own current/default status value at creation time (see §5).
- Mocked transition validation is typically exact string equality between the requested transition's expected starting status and the instance's actual current status; a stale in-memory model vs. the engine's real current status should throw immediately and leave the engine's actual state untouched. Explicitly test this corner case (single-hop mismatch, multi-hop drift, and repeated transitions chained off the same returned object) — these are easy to silently get wrong.

**Lookout:**
- Combo methods that both update fields and transition status must validate before any mutation occurs, in both possible orderings (update-then-transition and transition-then-update) — this is the same underlying bug class as the validation-bypass issue in §5/§7 and needs the same validator-injection fix applied to these methods specifically, since they often live in a hand-written partial class separate from the main generated CRUD methods.

---

## 11. Tests

**Do:**
- Standard scaffolding: an MSTest-based test project with parallelization disabled, a shared abstract base test class providing a mocked API helper via setup/cleanup hooks, a connection-mocking helper wrapping a DOM-engine test double, paired "populate" extension methods for seeding demo data, and a dedicated test class that sanity-checks the demo/fixture data itself (required fields present, natural keys actually unique, cross-references resolve, etc.).
- Test-class setup methods should only **construct** objects, never persist them — persistence should happen explicitly inside each test method that needs it. A setup method that unconditionally persists demo-like data will collide with any other test in the same class that separately calls a shared "populate" helper containing overlapping natural-key values (e.g. the same name used by both the manual setup data and the shared demo dataset).
- When a new mandatory business rule requires supporting fixture data to be added to test setup, grep first to find exactly which test files exercise the affected pipeline before editing broadly — some matches will be false positives (tests that only touch an unrelated entity type with no dependency on the new rule).
- When two modules' tests need to share one mocked connection/engine instance (e.g. one module's tests needing to resolve entities that live in another module), extend the shared connection-mocking helper with an additional method that builds the second module's helper against the same underlying mocked connection, rather than standing up a second, disconnected mock.
- For a test-class field that's only assigned inside `[TestInitialize]` (or another helper method), not inline or via a constructor, initialize it inline with `= null!;` at the declaration to suppress the nullable-reference CS8618 warning — this is the established convention across this test project's existing test classes; match it rather than introducing a different pattern (e.g. `#pragma warning disable`, `required`, or making the field nullable).

**Don't:**
- Don't leave throwaway probe/investigation test files in the tree once their purpose has been superseded by the real test suite — delete them as part of finishing that piece of work.

---

## 12. Commit / push hygiene

- Keep each commit to one clean, logically scoped diff; exclude any local tooling/index artifacts (e.g. generated index/junction folders) from version control.
- Include the required co-authorship trailer on every commit per repository convention.
- Get explicit confirmation before every commit — treat this as required every single time, even mid-flow after several individual pieces of work have already been separately approved.
- Show the exact push refspec on its own line and get explicit confirmation before every push — every time, no exceptions.
- Expect intermittent SSO-related push failures unrelated to the code itself in orgs with SAML SSO enforcement — this needs a browser-based re-authorization step outside the coding session, not a code fix.

---

## Common recurring risk categories to watch for on any future conversion work

1. **Enum serialization mismatches** — string vs. int storage per field, never assume uniformity (§2).
2. **Generated-artifact drift** — hand-maintained `.g.cs`/mapper files can silently diverge from the actual live DOM configuration (stale definition IDs, missing round-trip bookkeeping) if not deliberately kept in sync (§1, §5).
3. **Validation bypass through non-virtual same-class method calls** — any hand-written repository method that calls its own class's plain CRUD method directly, expecting a middleware/decorator wrapper to apply, is a latent bypass risk and should be checked explicitly wherever combo/composite operations exist (§5, §10).
4. **Struct-typed reference wrappers combined with `?.`** — a recurring, easy-to-reintroduce bug whenever null-conditional access is used on a member that returns a struct-based reference type (§3).
5. **Uniqueness/business-rule gaps hidden by a no-op or only-partial legacy implementation** — always verify by reading the actual legacy logic (including UI-only scripts) rather than trusting that a validation hook being present means the check is complete (§1, §7).
6. **N+1 database query loops in bulk validation** — a `ValidateBulk` Phase 3 loop that issues one DB (or remote API) query per item instead of one batched big-OR query for the whole collection; check for this explicitly whenever adding or reviewing bulk validation logic (§7).
7. **XML doc `cref` targeting an inherited-but-not-declaring interface** — `<see cref="Interface{T}.Member"/>` silently fails to resolve (CS1574) when `Member` is actually declared several interface-inheritance hops away; always target the interface that truly declares the member (§6).

---

## 13. Notes for AI agents using this document

**Scope of this document:** these rules come from hands-on investigation of one specific repo pairing — a hand-rolled legacy DOM wrapper library and a newer DOM model library that is nominally SDM-generated but currently frozen/hand-maintained. These are empirical findings, not vendor documentation. Re-verify preconditions before applying this wholesale to a different repo or a changed version of this one.

**Precondition checklist — run this before trusting §3–§5 as written:**
- Is the SDM SourceGenerator package actually referenced and *not* commented out in the target `.csproj`?
- Are `[SdmDomStorage]`/`[SdmDomMapper]`/`[GenerateExposers]`-style attributes live or commented out on the model class in question?
- Does `DomIds.cs` (or its equivalent) actually exist and contain populated entries for this module?

If any answer differs from "generator commented out / attributes disabled / `.g.cs` hand-maintained," several rules in §3 and §5 invert: a live generator run will reject unsupported list-element types instead of silently tolerating a hand patch, and direct edits to `.g.cs` files will get overwritten on next generation instead of being durable. Confirm this state explicitly at the start of any new work rather than assuming it matches this document.

**Verification-first mandate:** never state a legacy-behavior claim without having read the actual file and line. Do not infer legacy behavior from a sibling module's pattern by analogy — most of the bug classes catalogued here exist precisely because that assumption failed on inspection.

**Confidence tagging:** tag non-trivial claims as Certain (directly read the evidence), Likely (strong inference, not directly confirmed), or Guessing (filling a gap with no direct evidence). Preserve this tagging through to whatever gets reported back to a human — don't let a Guessing claim get restated as a confident fact by the time it's summarized.

**Escalation triggers — stop and ask instead of self-resolving when:**
- An unrelated build error surfaces mid-task.
- A newly discovered gap wasn't part of the original request.
- A decision affects live/persisted DOM data semantics (e.g. changing a field's storage type or a Definition ID).
- Before any commit.
- Before any push (show the literal refspec, get explicit confirmation).

**Tooling note:** use a .NET decompiler (e.g. `ilspycmd`) to inspect an external NuGet package's actual implementation when source isn't available locally. Prefer reading decompiled implementation over inferring library behavior from method or type names. When only the exact interface-inheritance chain/member signatures are needed (not full decompiled source), `ildasm.exe <dll> /out:dump.il /text` followed by grepping the dump for `.class interface` is faster and avoids missing-transitive-dependency failures that `Add-Type`/reflection-based inspection of multi-dependency SDK packages tends to hit in PowerShell.

**Reporting convention:** when reporting work back, separate three things explicitly — what changed, what was deliberately left untouched and why, and any newly discovered gap that is *not* what was originally asked for. Don't blend a new finding into the original ask's summary as if it were expected.
