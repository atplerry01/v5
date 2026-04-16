# Domain: Exposure

## Classification
economic-system

## Context
risk

## Domain Responsibility
Defines risk exposure positions — the tracked boundary of financial risk for a given source. This domain defines risk position structure only and contains no risk calculation logic.

## Aggregate
* **ExposureAggregate** — Root aggregate representing a risk exposure position.
  * Private constructor; created via `Create(ExposureId, SourceId, ExposureType, Amount, Currency, Timestamp)` factory method.
  * State transitions via `IncreaseExposure()`, `ReduceExposure()`, and `CloseExposure()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.

## Entities
* None

## State Model
```
Active ──ReduceExposure()──> Reduced ──IncreaseExposure()──> Active
Active ──CloseExposure()───> Closed (terminal)
Reduced ──CloseExposure()──> Closed (terminal)
```

## Value Objects
* **ExposureId** — Deterministic identifier (validated non-empty Guid).
* **ExposureStatus** — Enum: `Active`, `Reduced`, `Closed`.
* **ExposureType** — Enum: `Allocation`, `Obligation`, `Transaction`.
* **SourceId** — Typed reference to exposure source.

## Events
* **ExposureCreatedEvent** — Raised when exposure is created (status: Active).
* **ExposureIncreasedEvent** — Raised when exposure is increased.
* **ExposureReducedEvent** — Raised when exposure is reduced.
* **ExposureClosedEvent** — Raised when exposure is closed (terminal).

## Invariants
* ExposureId must not be null/default.
* SourceId must not be null/default.
* TotalExposure must be >= 0.
* Must not perform risk calculations — position structure only.
* State transitions enforced by specifications.

## Specifications
* **CanIncreaseSpecification** — Non-closed exposures can be increased.
* **CanReduceSpecification** — Non-closed exposures can be reduced.
* **CanCloseSpecification** — Non-closed exposures can be closed.
* **ExposureThresholdSpecification** — Validates exposure against threshold limit.

## Errors
* **MissingId** — ExposureId is required.
* **InvalidExposureAmount** — Amount must be positive.
* **AlreadyClosed** — Cannot modify a closed exposure.
* **ReductionExceedsTotal** — Cannot reduce below zero.
* **InvalidStateTransition** — Guard for illegal status transitions.

## Domain Services
* None — single-aggregate lifecycle; coordination handled by T2E engine handlers.

## Lifecycle Pattern
REVERSIBLE — Exposure can be increased and reduced; closed is terminal.

## Boundary Statement
This domain defines risk position structure only and contains no risk calculation logic.

## Status
**S4 — Invariants + Specifications Complete**

---

## E2 — Commands (contract: `Whycespace.Shared.Contracts.Economic.Risk.Exposure`)
* `CreateRiskExposureCommand(ExposureId, SourceId, ExposureType, InitialExposure, Currency, CreatedAt)`
* `IncreaseRiskExposureCommand(ExposureId, Amount)`
* `ReduceRiskExposureCommand(ExposureId, Amount)`
* `CloseRiskExposureCommand(ExposureId)`

## E3 — Queries
* `GetRiskExposureByIdQuery(ExposureId)`

## E4 — T2E Handlers (`Whycespace.Engines.T2E.Economic.Risk.Exposure`)
* `CreateRiskExposureHandler`
* `IncreaseRiskExposureHandler`
* `ReduceRiskExposureHandler`
* `CloseRiskExposureHandler`

## E5 — Policy Ids (`RiskExposurePolicyIds`)
* `whyce.economic.risk.exposure.create`
* `whyce.economic.risk.exposure.increase`
* `whyce.economic.risk.exposure.reduce`
* `whyce.economic.risk.exposure.close`

Bindings wired via `RiskPolicyModule.AddRiskPolicyBindings`.

## E6 — Event Fabric
**Topic:** `whyce.economic.risk.exposure.events`
**Channels (canonical):** `.commands`, `.events`, `.retry`, `.deadletter`
**Schemas:** `RiskExposureCreatedEventSchema`, `RiskExposureIncreasedEventSchema`,
`RiskExposureReducedEventSchema`, `RiskExposureClosedEventSchema`
(payload mappers registered in `EconomicSchemaModule.RegisterRiskExposure`).

## E7 — Projection
**Read model:** `RiskExposureReadModel`
**Store:** `projection_economic_risk_exposure.risk_exposure_read_model`
**Handler:** `RiskExposureProjectionHandler` (Inline execution policy)
**Consumer group:** `whyce.projection.economic.risk.exposure`

## E8 — API
* `POST /api/risk/exposure/create`
* `POST /api/risk/exposure/increase`
* `POST /api/risk/exposure/reduce`
* `POST /api/risk/exposure/close`
* `GET  /api/risk/exposure/{id}`

`ExposureId` generated deterministically via `IdGenerator.Generate($"economic:risk:exposure:{SourceId}:{ExposureType}:{Currency}")`.

## E9 — Workflow
**Not justified.** Exposure has a single-aggregate reversible lifecycle (Active ↔ Reduced → Closed) with no cross-domain compensation, approval flow, or long-running coordination. A T1M workflow would add orchestration without corresponding complexity. Deferred.

## E10 — Observability
* **Metrics:** `risk.exposure.count.total`, `risk.exposure.total_value` (sum by currency),
  `risk.exposure.status.count` (by status), `risk.exposure.breach.count`
  (when `ExposureThresholdSpecification` fails).
* **Tracing:** API → runtime → T2E handler → domain → event-store → projection.
  Correlation id propagated via `IEventEnvelope.CorrelationId`.
* **Structured logs:** command dispatch, policy decision, aggregate invariant outcome,
  projection upsert.

## E11 — Security & Enforcement
* All four commands require identity (`[Authorize]` on controller).
* WHYCEPOLICY evaluation per command via `CommandPolicyBinding` → `PolicyMiddleware`.
* Anti-bot / trust-score gating inherits from the runtime dispatcher pipeline.
* `ViolationDetectedEvent` may be emitted by the enforcement detection worker when
  exposure crosses configured thresholds (rego policy `whyce.enforcement.detect`).

## E12 — E2E Validation
Validation path: `POST /api/risk/exposure/create` → `ISystemIntentDispatcher` →
`CreateRiskExposureHandler` → `ExposureAggregate.Create` → event store → outbox →
Kafka (`whyce.economic.risk.exposure.events`) → `RiskExposureProjectionHandler` →
Postgres `projection_economic_risk_exposure.risk_exposure_read_model`.

Verification checklist:
* Policy decision recorded in `whyce.constitutional.policy.decision.events`.
* Event envelope persisted in event store with correct schema name.
* Kafka consumer advances offset under consumer group `whyce.projection.economic.risk.exposure`.
* Read-model row visible via `GET /api/risk/exposure/{id}`.
* Chain anchor present for each emitted event.

## E13–EX — Deferred
Cross-domain orchestration (capital ↔ risk), advanced economic routing, multi-cluster
distribution, AI-driven exposure prediction — all outlined in the batch prompt and
explicitly deferred.

