# Hash Determinism Guard

## Purpose

Lock the inputs to `ExecutionHash` and `DecisionHash` to a deterministic,
replay-stable set. Any change that introduces a timestamp, RNG value,
unordered collection, or non-normalized field into a hash input is a
critical violation. Hash determinism is the foundation of replay
verification — if hashes drift, replay cannot prove anything.

## Scope

- [src/runtime/deterministic/ExecutionHash.cs](../../src/runtime/deterministic/ExecutionHash.cs)
- [src/runtime/deterministic/DeterministicHasher.cs](../../src/runtime/deterministic/DeterministicHasher.cs) (if present)
- Any future `*Hash.cs` file under `src/runtime/deterministic/`
- The `DecisionHash` field/computation in policy evaluation
  ([src/engines/T0U/whycepolicy/](../../src/engines/T0U/whycepolicy/) and the policy result type)

## Permitted Hash Inputs

Only the following classes of input may feed a hash:

1. **Stable identifiers** — `correlationId`, `commandId`, `aggregateId`,
   `policyId`, `identityId`, `tenantId`. These are deterministic per
   command and reproduced exactly on replay.
2. **Normalized identity context** — `roles` (joined in canonical sort
   order), `trustScore` (string-formatted with invariant culture),
   `policyVersion` (string).
3. **Policy decision artifacts** — `policyDecisionHash`,
   `policyDecisionAllowed` (as a stable string).
4. **Domain event content** — event type names + per-event payload hashes
   computed via `DeterministicHasher.ComputePayloadHash(...)`. Events must
   be hashed in their emission order, with the position index included so
   that two events of the same type at different positions produce
   different signatures.
5. **Counts** — `domainEvents.Count` as a string. Counts are derived from
   the event list and are themselves deterministic.

## Forbidden Hash Inputs

The following are FORBIDDEN as direct or indirect inputs to any hash
computed in scope:

- `DateTime.Now`, `DateTime.UtcNow`, `DateTimeOffset.Now`,
  `DateTimeOffset.UtcNow` — wall clock readings vary per run.
- `IClock.UtcNow` — even though `IClock` is the canonical time seam, its
  value is not stable across replays. Time may be hashed by the *event
  payload* (because events carry their own deterministic stamps), but the
  hash function itself must not call into `IClock`.
- `Guid.NewGuid()` — non-deterministic by definition.
- `Random`, `Random.Shared`, `RandomNumberGenerator.GetBytes(...)` for
  hash salt — defeats determinism.
- `Environment.TickCount`, `Environment.TickCount64`,
  `Stopwatch.GetTimestamp()`.
- **Unordered collections** as inputs — `HashSet<T>`, `Dictionary<K,V>`,
  `ConcurrentDictionary<K,V>`, or any `IEnumerable<T>` that is not
  explicitly sorted before hashing. Iteration order of unordered
  collections is implementation-defined and may vary across runs and
  framework versions.
- **Non-normalized strings** — locale-sensitive `ToString()` calls
  (`(0.5).ToString()` rather than `(0.5).ToString(CultureInfo.InvariantCulture)`),
  case-sensitive comparisons of canonical identifiers without first
  applying a documented canonicalization, paths with mixed separators.
- **Reference equality** — hashing the result of `Object.GetHashCode()` or
  `RuntimeHelpers.GetHashCode()` couples the hash to memory layout.
- **Floating-point representations** — directly hashing `double`/`float`
  bit patterns. Use the canonical decimal string form via
  `InvariantCulture` round-trip format ("R" / "G17") only when absolutely
  necessary, and prefer `decimal` for any value that flows into a hash.

## Required Patterns

1. **Composite hashes** must be computed via `DeterministicHasher.ComputeCompositeHash(...)`
   (or equivalent). The composite function must concatenate inputs with a
   reserved separator that cannot appear in the inputs, so that
   `("ab", "c")` and `("a", "bc")` produce different hashes.
2. **Per-event signatures** must include the event's positional index, not
   just its type and payload. Two `TodoUpdatedEvent`s with the same payload
   at positions 0 and 1 must produce different per-event signatures.
3. **Sort before hash.** When hashing a collection, sort by a stable key
   first. The sort must be culture-invariant and document the comparator.
4. **Null sentinels.** When a field may be null, hash a fixed sentinel
   string (e.g. `"none"`, `"anonymous"`) rather than letting the null
   propagate. The sentinel must be unique enough that it cannot collide
   with a real value.

## Current Compliance (capture date 2026-04-07)

[ExecutionHash.cs:23-61](../../src/runtime/deterministic/ExecutionHash.cs#L23-L61)
is currently **compliant**. Inputs:

- `correlationId.ToString()`, `commandId.ToString()`, `aggregateId.ToString()` ✓ stable ids
- `identityId ?? "anonymous"`, `roles` joined, `trustScore?.ToString() ?? "0"` ✓ normalized identity
- `policyId`, `policyDecisionHash ?? "none"`, `policyDecisionAllowed?.ToString() ?? "false"`, `policyVersion ?? "none"` ✓ policy artifacts with sentinels
- `eventSignatures` built as `$"{type}:{i}:{payloadHash}"` per event ✓ position index included
- `domainEvents.Count.ToString()` ✓ derived count

**Zero forbidden inputs.** No clock read, no RNG, no unordered collection,
no locale-sensitive formatting. The file passes this guard by inspection.

## Check Procedure

1. Open every file in scope. Read the body of every `Compute*` /
   `Hash*` method.
2. For every input expression, classify as permitted or forbidden by the
   lists above.
3. Grep the scope for `DateTime`, `Guid.NewGuid`, `Random`, `HashSet<`,
   `Dictionary<`, `Environment.Tick`, `Stopwatch`. Each hit must be either
   absent from the hash inputs or justified by a documented sentinel
   pattern.
4. Verify per-event signatures include the position index when iterating
   a list of events.

## Pass Criteria

- Every hash input is in the permitted list or is a documented sentinel.
- No forbidden inputs reach a hash, directly or transitively.
- Per-event signatures are position-aware.
- Composite hashes use a reserved separator.

## Fail Criteria (S0 — CRITICAL)

- Any forbidden input reaches a hash function in scope.
- Per-event signatures lack position indexing.
- An unordered collection is hashed without an explicit sort.

## Severity

S0 — CRITICAL for any forbidden input. Hash drift is silent and breaks
replay verification without producing an error at the point of mistake.

## Enforcement Action

Block merge. Replay determinism cannot be claimed until the violation is
removed. The matching `replay-determinism.audit.md` must re-PASS.
