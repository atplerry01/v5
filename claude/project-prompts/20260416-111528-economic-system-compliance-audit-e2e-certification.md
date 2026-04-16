---
classification: economic-system
context: compliance
domain: audit
type: end-to-end-certification
stored: 2026-04-16T11:15:28Z
source: user-issued canonical certification prompt
---

# TITLE
End-to-End Validation & Certification — Domain Batch (Generic Canonical)

# CONTEXT
Full-system validation of the `economic-system / compliance / audit` domain (DOMAIN GROUP parameter `compliance` does not match canonical 3-level nesting; actual canonical path is `src/domain/economic-system/compliance/audit/`).

# OBJECTIVE
Perform a SYSTEM CERTIFICATION PASS confirming domain implementation, structural compliance, pipeline wiring, infrastructure integration (Postgres, Kafka, Redis, OPA, WhyceChain), determinism, policy enforcement, and production readiness.

# CLASSIFICATION
- CLASSIFICATION: economic-system
- CONTEXT: compliance
- DOMAIN GROUP (declared): compliance  ⚠ does not match canonical path
- DOMAINS: audit

# CONSTRAINTS
- Strict mode — no assumed correctness, evidence required at every layer
- Mandatory failure rule: any failure in determinism, policy, event persistence, kafka publishing, or projection update → SYSTEM = FAIL
- Output must be PASS / CONDITIONAL PASS / FAIL

# EXECUTION STEPS
0. Required Inputs
1. Validation Mode (strict)
2. Scope & Structure Validation
3. Domain Model Validation (E1 — S4 standard)
4. Command Layer Validation (E2)
5. Query Layer Validation (E3)
6. Engine Handler Validation (E4 — T2E)
7. Policy Integration Validation (E5)
8. Event Fabric Validation (E6) — Kafka topics
9. Postgres Validation (event store + projections)
10. Redis Validation
11. Workflow Validation (E9)
12. API Layer Validation (E8)
13. End-to-End Validation (E12) — full flow per domain
14. Observability Validation (E10)
15. Security & Enforcement (E11)
16. Final Certification Output

# OUTPUT FORMAT
- Overall Status
- Per-Domain Status
- Infrastructure Status (Postgres / Kafka / Redis / OPA)
- Critical Failures
- Non-Critical Gaps
- Evidence Summary
- Certification Decision (APPROVED FOR PHASE PROGRESSION or BLOCKED)

# VALIDATION CRITERIA
See sections 2–15 of source prompt; certification decision driven by section 16 + Mandatory Failure Rule.

# ORIGINAL PROMPT
(See conversation; verbatim certification prompt issued by the user on 2026-04-16.)
