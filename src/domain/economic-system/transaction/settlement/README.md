# Domain: Settlement

## Classification
economic-system

## Context
transaction

## Domain
settlement

## Purpose
Models the **external execution boundary** for transaction value movement —
bank rails, payment providers, ACH, card settlement, etc. The aggregate is a
pure event-sourced write model: it validates settlement-local invariants
(positive amount, mandatory source reference, strict lifecycle) and emits
events. Runtime is responsible for persistence, Kafka publication, projection,
and WhyceChain anchoring. Actual external execution (HTTP to providers, SDK
calls) is performed by adapters/workers outside the domain — the domain only
tracks the reference and status.

Complements (does not replace) `economic-system/ledger/settlement`, which is
the internal-truth settlement variant used by ledger reconciliation.

## Owns
- The canonical lifecycle of an external settlement attempt
- Settlement-local invariants (amount, currency, source reference, provider)
- Deterministic state transitions between
  Initiated / Processing / Completed / Failed
- The immutable `SettlementReference` attached on completion

## Does NOT Own
- Actual rail / provider execution (adapter / worker concern)
- Ledger posting against the settlement (ledger domain)
- Capital balance impact (capital domain)
- Persistence, publication, or chain anchoring (runtime)

## Aggregate
- **SettlementAggregate** — event-sourced, sealed. Emits
  `SettlementInitiatedEvent`, `SettlementProcessingStartedEvent`,
  `SettlementCompletedEvent`, `SettlementFailedEvent`.

## Entities
- **SettlementReference** — immutable external-execution attestation,
  attached on completion (provider + external transaction id + metadata).

## Value Objects
- **SettlementId** — typed Guid wrapper, `From()` factory, non-empty invariant
- **SettlementStatus** — enum (Initiated, Processing, Completed, Failed)
- **SettlementAmount** — decimal wrapper, strictly positive
- **SettlementCurrency** — normalized upper-case currency code
- **SettlementSourceReference** — mandatory non-empty reference to the
  internal source (ledger entry / transaction / capital movement)
- **SettlementReferenceId** — external transaction id assigned by the rail
- **SettlementProvider** — opaque rail / provider identifier

## Domain Events
- **SettlementInitiatedEvent** (SettlementId, Amount, Currency, SourceReference, Provider)
- **SettlementProcessingStartedEvent** (SettlementId)
- **SettlementCompletedEvent** (SettlementId, ExternalReferenceId)
- **SettlementFailedEvent** (SettlementId, Reason)

## Specifications
- **SettlementSpecification** — amount > 0, non-empty source reference,
  non-empty provider
- **SettlementLifecycleSpecification** — allowed transitions
  (Initiated -> Processing, Processing -> Completed, Processing -> Failed)
  plus `CanProcess / CanComplete / CanFail / PreventReversal` helpers

## Domain Services
- **SettlementService.ValidateExternalReference(...)** — pure structural
  validation of the external reference id
- **SettlementService.ValidateConsistency(settlement)** — pure consistency
  check that a Completed settlement carries its reference

## Errors
Strongly-typed via `SettlementErrors` static:
- `InvalidAmount`
- `InvalidStateTransition(from, to)`
- `MissingSourceReference`
- `AlreadyCompleted`
- `AlreadyFailed`
- `NegativeAmount` (invariant)
- `TerminalStateMutation` (invariant)

## Lifecycle

```
Initiate(...)         -> Initiated
MarkProcessing()      -> Processing   (only from Initiated)
MarkCompleted(refId)  -> Completed    (only from Processing, TERMINAL)
MarkFailed(reason)    -> Failed       (only from Processing, TERMINAL)
```

## Invariants (CRITICAL)
- Amount must be strictly greater than zero at creation.
- Amount must never be negative (`EnsureInvariants`).
- SourceReference must be non-empty.
- Status transitions restricted to the lifecycle specification.
- **Completed and Failed are terminal — the aggregate rejects any further
  mutation attempt via `GuardNotTerminal()`.**
- No external SDK / HTTP / Kafka / DB access anywhere in the domain.

## Integration Domains
- **ledger** — consumes `SettlementCompletedEvent` / `SettlementFailedEvent`
  to reconcile posted journal entries against external execution
- **capital** — consumes terminal settlement events for balance resolution
- **reconciliation** — consumes the full settlement event stream as the
  external truth anchor

All integrations flow strictly through events; this aggregate has zero
dependencies on any other BC.

## Runtime Flow
```
POST /api/settlement/{initiate|complete|fail}
  -> SettlementController
  -> ISystemIntentDispatcher (DomainRoute: economic/transaction/settlement)
  -> RuntimeControlPlane (T0U policy gate)
  -> T2E {Initiate|Complete|Fail}SettlementHandler
  -> SettlementAggregate.{Initiate|MarkProcessing|MarkCompleted|MarkFailed}
  -> DomainEvents emitted via IEngineContext.EmitEvents
  -> Runtime persists event store row, outbox row, publishes to
     whyce.economic.transaction.settlement.events, anchors chain block
  -> SettlementProjectionHandler reduces into SettlementReadModel
```

## Determinism
- SettlementId is derived via `IIdGenerator` at the entry point — no
  `Guid.NewGuid()`.
- No `DateTime.UtcNow` — all temporal stamping is runtime-owned via
  envelope metadata.

## External Execution Boundary
The domain is the **boundary of truth**, not the boundary of execution.
Real-world money movement happens OUTSIDE this domain in:
- Infrastructure adapters (`src/platform/host/adapters/**`)
- Background workers
- Third-party SDK integrations

The domain's job is to track what was attempted, what succeeded, and what
failed — with irreversible, append-only semantics that make reconciliation
possible.

## Notes
- The aggregate is deliberately minimal; cross-aggregate mutations are
  intentionally absent and must be orchestrated outside the domain.
- Kafka topic: `whyce.economic.transaction.settlement.{commands|events|retry|deadletter}`.
