---
classification: economic-system
context: compliance
domain: audit
type: s0-recovery-patch
stored: 2026-04-16T11:30:40Z
follows: claude/project-prompts/20260416-111528-economic-system-compliance-audit-e2e-certification.md
scope: surgical (infrastructure + policy binding + minimal tests only)
---

# TITLE
S0 Recovery Patch — economic-system / compliance / audit

# CONTEXT
Resolves the five blockers raised in the prior certification (FAIL → PASS without modifying domain logic).

# CURRENT FAILURES (input)
- C1 — R-K-17 / R-K-20 / K-TOPIC-COVERAGE-01 (S0): Kafka topic `whyce.economic.compliance.audit.events` not declared.
- C2 — Projection table `projection_economic_compliance_audit.audit_record_read_model` not materialized (S0).
- C3 — PB-08 policy source unverified (S0 if unresolved).
- C4 — `read` policy declared but unbound (POL-03, S0).
- C5 — No automated tests (S1).

# CONSTRAINTS
- ALLOWED: `infrastructure/event-fabric/kafka/create-topics.sh`, projection DDL, policy module bindings, `tests/**`, OPA / policy-source wiring.
- FORBIDDEN: changes under `src/domain/**`, runtime pipeline, handlers, controllers, policy redesign.

# EXECUTION STEPS
1. Add `whyce.economic.compliance.audit.{commands,events,retry,deadletter}` to `create-topics.sh`.
2. Create `infrastructure/data/postgres/projections/economic/compliance/audit/001_projection.sql`.
3. Bind read policy in `CompliancePolicyModule` (or via a query-policy seam if available).
4. Verify PB-08 — confirm external policy source for the three audit policy ids; document.
5. Add three tests (aggregate lifecycle, handler-level dispatch, projection reducer).

# OUTPUT FORMAT
- Updated `create-topics.sh`
- New projection migration
- Policy binding addition + PB-08 verification note
- 3 new test files
- Re-run audit sweep + certification re-check report

# VALIDATION CRITERIA
Validation checklist (§5 of source prompt) all green; no domain modifications; certification status flips to PASS.

# ORIGINAL PROMPT
(Verbatim recovery batch issued 2026-04-16; see conversation.)
