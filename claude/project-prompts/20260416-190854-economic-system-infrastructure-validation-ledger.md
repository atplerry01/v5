---
classification: economic-system
context: ledger
domain-group: ledger
domains-requested: [transactions, balances, accounts]
domains-actual: [entry, journal, ledger, obligation, treasury]
mode: dry-run artefact verification + plan generation (no service bring-up)
routing: standalone (not /pipeline/** driven)
timestamp: 20260416-190854
author: atplerry@gmail.com
---

# TITLE

Infrastructure Validation — Real Execution (Docker / Kafka / Postgres / OPA / Full Pipeline) — Ledger target

## CONTEXT

Validation of the canonical write→event→projection→read pipeline for the `economic-system / ledger` context. User-supplied domain list (`transactions, balances, accounts`) did **not** match the on-disk structure. The actual `src/domain/economic-system/ledger/` contains: `entry, journal, ledger, obligation, treasury`. Validation was scoped to the actual domains (with the mismatch captured as a finding).

## OBJECTIVE

Prove that the full system pipeline — API → Dispatcher → Runtime → Policy → Engine → Event Store → Outbox → Kafka → Projection → API — executes correctly against real infrastructure (Docker + Postgres + Kafka + OPA), for every ledger sub-domain.

This invocation performs dry-run artefact verification only; the generated plan is the script for the live run.

## CONSTRAINTS

- No mocks / no in-memory substitutes.
- Canonical topic naming: `whyce.{classification}.{context}.{domain}.{channel}` per R-K-18.
- Must verify determinism, idempotency, WHYCEPOLICY enforcement, projection correctness.
- $1a guards fully loaded before any execution (constitutional, runtime, domain, infrastructure).
- $2 prompt stored at this path before execution proceeds.
- No code changes in this invocation (dry-run + plan only).

## EXECUTION STEPS

1. Load all guards under `claude/guards/**` (constitutional, runtime, domain, infrastructure).
2. Snapshot the ledger target artefact surface (docker-compose, topics, schemas, mappers, projections, API, policies, bootstrap).
3. Classify each artefact as PRESENT / MISSING / PARTIAL.
4. Emit the validation plan (PHASE 1..10) parameterised to the actual sub-domains.
5. Emit the dry-run report (per-sub-domain readiness, gaps, severity).
6. Run post-execution audit sweep per $1b.

## OUTPUT FORMAT

- Plan file: `claude/project-prompts/20260416-190854-economic-system-infrastructure-validation-ledger.plan.md`
- Dry-run report: inline in conversation.
- New-rules captures (if any) under `claude/new-rules/` per $1c.

## VALIDATION CRITERIA

- All four canonical guards loaded without contradiction.
- Every sub-domain enumerated with a PRESENT/MISSING status for: topic declaration, topic metadata, schema, payload mapper, projection handler, projection SQL, API controller, OPA policy, bootstrap module.
- No architectural change proposed (per $5 Anti-Drift).
- Final status one of: APPROVED / CONDITIONAL PASS / FAIL, with rationale.