---
TITLE: End-to-End Validation & Certification — Capital Domain Batch
CONTEXT: economic-system/capital — 7 domains: account, allocation, asset(s), binding, pool, reserve, vault
OBJECTIVE: System certification pass per validation-prompt.md (E1–E12 layers)
CONSTRAINTS: Strict mode, no partial validation, minimum S4 domain standard, mandatory failure rule applies
EXECUTION STEPS: Guard load ($1a) → structure → per-domain model (E1) → commands (E2) → queries (E3) → handlers (E4) → policy (E5) → events/Kafka (E6) → Postgres/outbox (E7/E12) → Redis → workflows (E9) → API (E8) → E2E (E12) → observability (E10) → security (E11) → audit sweep ($1b)
OUTPUT FORMAT: Section 16 — PASS/CONDITIONAL PASS/FAIL with per-domain + infra status
VALIDATION CRITERIA: MANDATORY FAILURE RULE — any failure in determinism / policy / event persistence / kafka publishing / projection update = SYSTEM FAIL
CLASSIFICATION: economic-system / capital / validation-certification
---

# Prompt Body

(verbatim reference: c:\projects\dev\v5\validation-prompt.md, lines 1–460, loaded 2026-04-15 20:21:28)

Validates economic-system/capital domain batch across all canonical execution layers.
