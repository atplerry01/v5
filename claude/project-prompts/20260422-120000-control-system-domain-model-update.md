# Control-System Domain Model Update — Full E1→EX Delivery

## TITLE
control-system: full domain model update using canonical topics (all contexts)

## CONTEXT
CLASSIFICATION: control-system
CONTEXT: all contexts (system-policy, configuration, access-control, audit, observability, scheduling, system-reconciliation)
DOMAIN GROUP: per-context (see below)
DOMAINS: all missing domains per context per control-system.md topics

Source topics: /claude/project-topics/v3/control-system.md
Template: /claude/templates/pipelines/generic-prompt.md

## OBJECTIVE
Align the control-system domain layer with the canonical topics file by implementing all missing domains across all contexts, following the E1→EX delivery model. Update existing partial contexts and scaffold the new system-reconciliation context.

## CONSTRAINTS
- Domain layer only (E1 stage primary); all layers per generic-prompt structure
- No external/infrastructure dependencies inside domain
- No Guid.NewGuid(), DateTime.UtcNow, or non-deterministic patterns
- All naming, paths, namespaces, and topic contracts canonical
- Align with WhyceID, WHYCEPOLICY, WhyceChain doctrine
- Follow DS-R3a domain-group discipline (control-system is 3-level flat per context)
- system-reconciliation context: optional per topics, implement as D2
- scheduling context: guard-constrained to administrative job/schedule control only (no process-orchestration or workflow-coordination per domain.guard.md)

## EXECUTION STEPS
1. Batch B.1 — system-policy: policy-package, policy-evaluation, policy-audit
2. Batch B.2 — access-control: identity, principal, access-policy
3. Batch B.3 — configuration: configuration-assignment
4. Batch C.1 — audit: audit-event, audit-trace, audit-query
5. Batch C.2 — observability: system-signal, system-health
6. Batch D.1 — system-reconciliation: consistency-check, discrepancy-detection, discrepancy-resolution, reconciliation-run, system-verification

## GAP ANALYSIS (existing vs. topics)

### system-policy context
- EXISTING: policy-definition, policy-decision, policy-enforcement
- MISSING: policy-package, policy-evaluation, policy-audit

### access-control context
- EXISTING: role, permission, authorization
- MISSING: identity, principal, access-policy

### configuration context
- EXISTING: configuration-definition, configuration-state, configuration-scope, configuration-resolution
- MISSING: configuration-assignment

### audit context
- EXISTING: audit-log, audit-record
- MISSING: audit-event, audit-trace, audit-query

### observability context
- EXISTING: system-metric, system-alert, system-trace
- MISSING: system-signal, system-health

### scheduling context (topics calls it orchestration; guard renamed to scheduling)
- EXISTING: execution-control, schedule-control, system-job
- NOTE: topics list process-orchestration and workflow-coordination — EXCLUDED per domain.guard.md "administrative job/schedule control only"

### system-reconciliation context (optional)
- EXISTING: nothing (context does not exist)
- MISSING: all 5 domains

## OUTPUT FORMAT
Production-ready C# domain files following canonical patterns:
- aggregate/{Domain}Aggregate.cs
- error/{Domain}Errors.cs
- event/{Domain}Event(s).cs
- specification/.gitkeep (unless business rules justify a spec)
- value-object/{X}.cs
- README.md

## VALIDATION CRITERIA
- All missing domains created at D2 level
- No infrastructure leakage
- No non-deterministic APIs
- All invariants explicit
- All events past-tense
- Namespace pattern: Whycespace.Domain.ControlSystem.{Context}.{Domain}
- system-reconciliation context README present
