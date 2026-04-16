# TITLE
Production-grade Whycespace domain implementation — content-system/media/asset

# CONTEXT
User requested a WBSM v3.5 production-grade domain scaffold at
`content-system/{context}/{domain}` with example `content-system/media/asset`.
Per memory/feedback (Decide and proceed), concrete target selected:
`content-system/media/asset`. Existing domain baseline (e.g.
`operational-system/activation`) is stub-thin; this prompt materialises the
first full content-system domain against the shared-kernel primitives
(`AggregateRoot`, `DomainEvent`, `DomainException`,
`DomainInvariantViolationException`, `Specification<T>`, `Timestamp`,
`Guard`).

# OBJECTIVE
Deliver the S4 Domain Standard folder set for `content-system/media/asset`
with aggregate, events, value-objects, specifications, errors, and README.
Domain must be pure (no infra, no DB, no clock, no Guid.NewGuid), event-
sourced, deterministic, invariant-enforcing, and immediately compilable
against existing shared-kernel types.

# CLASSIFICATION
- classification: content-system
- context: media
- domain: asset

# CONSTRAINTS
- S4 folder layout exactly: aggregate, event, error, specification,
  value-object (+ README.md). Entity/service omitted — not required.
- Domain purity: `src/domain/**` references only
  `Whycespace.Domain.SharedKernel.*`. No infra, no engines, no runtime.
- Determinism: no `Guid.NewGuid()`, no `DateTime.*Now`, no `Random`.
  Identifiers accepted as parameters (injected upstream by HSID /
  IDeterministicIdEngine / IIdGenerator).
- Events: past-tense `{Subject}{Action}Event`, inherit from `DomainEvent`,
  immutable records. Per user prompt each event carries EventId,
  AggregateId, CorrelationId, CausationId (as Guid-backed value types
  already defined in shared-kernel).
- Errors: centralised in `MediaAssetErrors` returning `DomainException` /
  `DomainInvariantViolationException`. No generic exceptions.
- Specifications: extracted validation via `Specification<T>`.
- Aggregate: sealed, inherits `AggregateRoot`, `EnsureInvariants` enforces
  business rules, state changes only through `RaiseDomainEvent` + `Apply`.
- Namespace: `Whycespace.Domain.ContentSystem.Media.Asset`.

# EXECUTION STEPS
1. $1a pre-execution guard load (constitutional, runtime, domain,
   infrastructure).
2. Persist this prompt under `/claude/project-prompts/`.
3. Create folder set under `src/domain/content-system/media/asset/`.
4. Author value-objects, events, errors, specifications, aggregate,
   README.md.
5. $1b post-execution audit sweep against the new files.
6. Capture any new-rule drift under `/claude/new-rules/` per $1c.

# OUTPUT FORMAT
- Full folder tree
- Full source for aggregate / events / value-objects / specifications /
  errors
- README.md
- Audit sweep summary

# VALIDATION CRITERIA
- S4 folder completeness
- Zero references to infrastructure / framework types
- Zero `Guid.NewGuid()` / `DateTime.Now` / `DateTime.UtcNow`
- All events past-tense, immutable, inherit `DomainEvent`
- Aggregate sealed, `Version` versioning inherited, invariants enforced
- Errors centralised, no generic exceptions
- Specifications used for validation
- README lifecycle + event flow + invariants documented
