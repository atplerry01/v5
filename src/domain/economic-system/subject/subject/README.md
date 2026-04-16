# Economic Subject Domain

Classification: `economic-system`
Context: `subject`
Domain Group: `subject`
Domain: `subject`

## Purpose

Bridges structural-system actors (Cluster, Subcluster, SPV, Provider, Participant)
into the economic domain by registering them as economic subjects. Each economic
subject carries:

- `SubjectType` — its role at the economic boundary (Participant, CWG, SPV,
  Provider, Cluster). Note that `CWG` is an alias of structural `Subcluster`
  surfaced only at the economic boundary; no structural rename occurs.
- `StructuralRef` — the canonical reference back to the structural actor.
- `EconomicRef` — the canonical reference to the economic artifact that owns
  the subject's balance (`VaultAccount` for Cluster/Subcluster/SPV,
  `CapitalAccount` for Participant/Provider).

## Aggregate

`EconomicSubjectAggregate` is the aggregate root. Registration is the single
lifecycle transition in S4 scope. The aggregate emits
`EconomicSubjectRegisteredEvent` on success.

## State Model

- `Registered` — the only materialised state. Re-registration is not permitted
  (handled upstream via idempotency on `SubjectId`).

## Events

- `EconomicSubjectRegisteredEvent`

## Invariants

- `StructuralRef` is required.
- `EconomicRef` is required.
- `EconomicRef.RefType` MUST be consistent with `SubjectType` per
  `EconomicRefRules` / `EconomicRefMatchesSubjectSpecification`.

## Errors

- `SubjectErrors.AlreadyRegistered`
- `SubjectErrors.MissingStructuralRef`
- `SubjectErrors.MissingEconomicRef`

## Commands

- `RegisterEconomicSubjectCommand`

## Queries

- `GetEconomicSubjectByIdQuery`

## Projection / Read Model

- `EconomicSubjectReadModel`
  - Schema: `projection_economic_subject_subject`
  - Table: `economic_subject_read_model`
  - Aggregate type: `EconomicSubject`

## Policy Actions

- `whyce.economic.subject.subject.register`

## Topic

- `whyce.economic.subject.subject.events`

## API Surface

- `POST /api/economic/subject/register` — register a new economic subject
- `GET  /api/economic/subject/{id}` — fetch an economic subject read model

## Canonical Path

- Domain:       `src/domain/economic-system/subject/subject/`
- Commands:     `src/shared/contracts/economic/subject/subject/`
- Events:       `src/shared/contracts/events/economic/subject/subject/`
- Handler:      `src/engines/T2E/economic/subject/subject/`
- Projection:   `src/projections/economic/subject/subject/`
- Controller:   `src/platform/api/controllers/economic/subject/subject/`
- Composition:  `src/platform/host/composition/economic/subject/...`

## E2E Path

API → Dispatcher → Runtime Control Plane → RegisterEconomicSubjectHandler →
EconomicSubjectAggregate → EventStore → WhyceChain Anchor → Outbox →
`whyce.economic.subject.subject.events` → SubjectProjectionHandler →
`projection_economic_subject_subject.economic_subject_read_model` → Response.
