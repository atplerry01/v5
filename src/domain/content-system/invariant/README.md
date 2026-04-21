# content-system — Cross-System Invariant Layer

Domain-level policies that span multiple bounded contexts inside the
content-system classification (and cross-reference business-system where
required). Implemented per the E1→EX delivery template:
`claude/templates/delivery-pattern/01-domain-skeleton.md` §
Cross-System Invariants.

## Why this layer exists

A single aggregate cannot guarantee consistency across BCs it does not own.
Cross-system invariants encode the **sixth pillar of truth** — consistency
between systems — alongside structural, business, content, economic, and
operational truth.

## Policies

| Policy | Spans | Rule |
|---|---|---|
| [ContentMustHaveValidOwnerPolicy](ownership/policy/ContentMustHaveValidOwnerPolicy.cs) | content-system × business-system | Any content artefact (document, media, stream) must have a valid, non-empty business owner before activation. |
| [BroadcastStreamBindingPolicy](broadcast-stream-binding/policy/BroadcastStreamBindingPolicy.cs) | streaming/live-streaming/broadcast × streaming/stream-core/stream | A Broadcast must reference a Stream that exists and is not in a terminal (`Ended`/`Archived`) state. |
| [SessionStreamAccessPolicy](session-stream-access/policy/SessionStreamAccessPolicy.cs) | streaming/playback-consumption/session × streaming/delivery-governance/access | A Session activation requires a non-revoked, non-expired, non-restricted access grant for the (subject, stream) pair. |

## Enforcement

Each policy is a pure decision function (`Allow` | `Deny + reason`). They
are invoked by T2E handlers **before** aggregate mutation — see
`02-engine-skeleton.md` § Cross-System Invariants (enforcement mechanism).

On `Deny`, the handler throws `DomainInvariantViolationException` (maps to
HTTP 400 via the existing `DomainExceptionHandler` middleware) and emits no
event. On `Allow`, the handler proceeds to aggregate mutation.

### Current enforcement status

| Policy | Enforcement point | Status |
|---|---|---|
| `ContentMustHaveValidOwnerPolicy` | pre-existing | pre-existing — see policy class for current invocation |
| `BroadcastStreamBindingPolicy` | [CreateBroadcastHandler](../../../../engines/T2E/content/streaming/live-streaming/broadcast/CreateBroadcastHandler.cs) | **enforced** — loads stream status via [IStreamStatusLookup](../../../../shared/contracts/content/streaming/stream-core/stream/IStreamStatusLookup.cs); throws on Deny. |
| `SessionStreamAccessPolicy` | `ActivateSessionHandler` | **registered-but-not-enforced** — the `SessionAggregate` does not yet carry a subject reference, and `ActivateSessionCommand` does not carry an access-grant id. Enforcement requires either (a) adding a subject VO to the Session aggregate + a subject-indexed access-grant projection, or (b) threading the access-grant id through `OpenSessionCommand`. Captured as a follow-up. |

### Cross-BC state lookups

For cross-system invariants that need projection-derived facts from another
BC, the handler takes a state-lookup abstraction as a constructor
dependency. The abstraction is declared under
`src/shared/contracts/content/streaming/{group}/{bc}/` and implemented as a
thin adapter over the `PostgresProjectionStore<T>` under
`src/projections/content/streaming/{group}/{bc}/`. This keeps the engine
handler infra-agnostic while allowing the adapter to query the read model.

Current lookups:

- [`IStreamStatusLookup`](../../../../shared/contracts/content/streaming/stream-core/stream/IStreamStatusLookup.cs) — used by `BroadcastStreamBindingPolicy` enforcement.

## Rules

1. **Pure.** No `DateTime.*`, no `Guid.NewGuid()`, no DI, no BCL exceptions,
   no I/O. Same discipline as aggregates.
2. **No VO imports.** Policies accept primitive / projection-derived facts
   via `*Input` records so they do not couple cross-BC VO types.
3. **Enforce once.** Choose one enforcement point per command — inline in
   T2E handler OR as a T1M step. Never both.
4. **No replay dependency.** Policies are evidence at emission time; on
   replay, events are trusted, the policy is not re-evaluated.

## Registration

Policies are registered in `ContentSystemCompositionRoot` as singletons.
See `03-runtime-wiring.md` § 6b.
