TITLE: End-to-End Validation & Certification — economic-system/ledger Domain Batch

CONTEXT:
Invoked 2026-04-15 22:52:20 UTC against domain group `ledger` under `economic-system`. Scope: 6 domains (entry, journal, ledger, obligation, settlement, treasury). Validation authority: WBSM v3 canonical rules (CLAUDE.md) + the four canonical guards under `claude/guards/` (constitutional, runtime, domain, infrastructure).

OBJECTIVE:
Full SYSTEM CERTIFICATION PASS per `validation-prompt.md` — structural, domain model, command/query, engine handler, policy, Kafka, Postgres, Redis, workflow, API, E2E, observability, and security validation with evidence-per-layer. Return PASS / CONDITIONAL PASS / FAIL with per-domain + per-infrastructure verdict.

CONSTRAINTS:
- $1a: Guards loaded (constitutional + infrastructure fully; runtime + domain indexed).
- $5: Anti-drift — no inferred components; missing parts reported as gaps, not synthesized.
- $9: Determinism checks applied (Guid.NewGuid / DateTime.UtcNow forbidden in scope).
- $11: Evidence required per layer; silent conformance not accepted.
- Mandatory failure rule: determinism / policy / event-persist / kafka-publish / projection-update failure → SYSTEM FAIL.

EXECUTION STEPS:
1. Structural sweep for 6 domains under `src/domain/economic-system/ledger/**`.
2. Parallel evidence gathering (domain purity, commands, queries, engine handlers, policy bindings, kafka topics, postgres migrations, redis usage, api controllers, observability) via Explore subagents.
3. Synthesize per-section PASS/FAIL with cited file paths.
4. Produce Final Certification Output per section 16 of the prompt.
5. Run post-execution audit sweep per $1b.

OUTPUT FORMAT:
Final Certification block with: Overall Status, Per-Domain Status (6), Infrastructure Status (Postgres/Kafka/Redis/OPA), Critical Failures, Non-Critical Gaps, Evidence Summary, Certification Decision.

VALIDATION CRITERIA:
Every claim cites a file path. No assumption of correctness. Any S0/S1 from any canonical guard → FAIL.
