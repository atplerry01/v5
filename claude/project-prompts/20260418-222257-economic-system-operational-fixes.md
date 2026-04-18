# Economic-System Operational Fixes (Scenario A remediation)

## CLASSIFICATION
- phase: phase5-operational-activation
- classification: economic-system
- domain: all (capital, compliance, enforcement, exchange, ledger, reconciliation, revenue, risk, routing, subject, transaction, vault)
- type: defect-fix + stabilization

## CONTEXT

Follow-up to `claude/project-prompts/20260418-121953-economic-operational-validation.md`. Scenario A (functional success path) produced 3 PASS · 5 WARNING · 4 FAIL across 12 contexts. Four defects captured as `OPS-VAL-001..004` in `claude/new-rules/20260418-214500-audits.md` and echoed in `docs/operational-validation/economic-system/_readiness-summary.md`.

## OBJECTIVE

Resolve the four defects at root cause with minimal surgical changes. Re-run Scenario A. Ship only if all 12 contexts PASS or WARNING (intentional policy denial), with zero FAIL and zero silent-loss WARNING.

## CONSTRAINTS ($5 anti-drift)

- No new architecture. No new patterns. No renames. No file moves. No inferred components.
- No change to domain invariants unless required for correctness.
- No policy weakening. Deny-by-default remains.
- No change to unrelated contexts (capital, ledger, vault remain untouched).
- Fix root causes only.

## EXECUTION STEPS

1. Load 4 canonical guards per $1a.
2. **Fix 1 (S0, exchange)** — idempotent outbox insert.
   - Read `ExchangeControllerBase.Dispatch`, `RegisterFxPairCommandHandler`, outbox adapter.
   - Identify: is the outbox `id` deterministic (collides on retry), or does duplicate-command short-circuit occur *after* the insert?
   - Apply minimal fix: short-circuit before insert OR `ON CONFLICT DO NOTHING` on the outbox insert keyed by `idempotency_key` (the existing unique index `uq_outbox_idempotency`).
3. **Fix 2 (S2, compliance/reconciliation/subject)** — 500→400 on enum parse.
   - Catch `ArgumentException` / `InvalidOperationException` at controller DTO-to-command conversion and return `BadRequest(ApiResponse.Fail(...))`.
4. **Fix 3 (S1, routing silent loss)** — kafka-init race.
   - Add `kafka-init: condition: service_completed_successfully` to host service `depends_on` in both `infrastructure/deployment/docker-compose.yml` and `infrastructure/deployment/multi-instance.compose.yml`.
   - Confirms: a host service cannot accept traffic until every declared topic exists.
5. **Fix 4 (testability)** — `ops-validator` role permissions.
   - Add explicit permit rules for the four policy-blocked endpoints (lock, create-contract, create-exposure, calculate-charge) for role `ops-validator`.
   - Do NOT modify default-deny behavior.
   - Mint JWT with `roles=["ops-validator"]` for the re-run.
6. Re-run Scenario A across all 12 contexts.
7. Produce updated readiness summary.
8. Capture any new drift per $1c. Run audit sweep per $1b.

## OUTPUT FORMAT

- Files-modified list (minimal).
- Summary of each fix.
- Updated Scenario A classification table.

## VALIDATION CRITERIA

- Exchange: repeated `POST /api/exchange/fx/register` with identical body returns consistent result, no 500, no duplicate outbox effects.
- Compliance/reconciliation/subject: invalid enum value returns 400 with `ApiResponse.Fail` shape.
- Routing: host services do not serve requests until `kafka-init` has exited successfully.
- Enforcement/revenue/risk/transaction: valid `ops-validator` token yields PASS; missing/invalid token still yields 401/403.
- Capital, ledger, vault remain PASS.
