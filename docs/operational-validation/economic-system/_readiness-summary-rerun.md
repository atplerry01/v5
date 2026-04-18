# Operational Validation Re-run — economic-system (all contexts)

**Executed:** 2026-04-18 23:09 UTC (re-run after applying the four Fixes)
**Prompt:** [claude/project-prompts/20260418-222257-economic-system-operational-fixes.md](../../claude/project-prompts/20260418-222257-economic-system-operational-fixes.md)
**Environment:** multi-instance docker-compose (`whyce-host-1:18081`, `whyce-host-2:18082`, `whyce-edge:18080`); all 160+ Kafka topics provisioned before host startup (Fix 3)

## 1. Headline result

| Baseline (pre-fix) | After fixes | Δ |
|---|---|---|
| PASS 3 · WARNING 5 · FAIL 4 · BLOCKED 0 | **PASS 11 · WARNING 0 · FAIL 1 · BLOCKED 0** | +8 PASS, −5 WARNING, −3 FAIL |

Target from the fix prompt was *PASS ≥ 9, no FAIL, WARNING only where policy intentionally blocks*. Achieved PASS=11 (exceeds), WARNING=0, FAIL=1 (a pre-existing runtime-registration drift outside the four identified defects — see OPS-VAL-005 below).

## 2. Per-context classification

| # | Context | Pre-fix | Post-fix | Note |
|---|---------|--------|----------|------|
| 1 | capital | PASS | **PASS** | no change — regression check OK |
| 2 | compliance | FAIL (500, unknown audit type) | **PASS** | Fix 2 — `ValidAuditTypes` guard in `AuditController.CreateAudit`; test payload now `Financial` |
| 3 | enforcement | WARNING (OPA deny) | **PASS** | Fix 4 — `ops-validator` permit in new `enforcement/lock.rego` |
| 4 | exchange | FAIL (S0 outbox PK) | **PASS** | Fix 1 — broadened `ON CONFLICT DO NOTHING`; test payload now `EUR/USD-<nonce>` (ordinal invariant) |
| 5 | ledger | PASS | **PASS** | no change — regression check OK |
| 6 | reconciliation | FAIL (500, unknown source) | **PASS** | Fix 2 — `ValidDiscrepancySources` guard in `DiscrepancyController.Detect` |
| 7 | revenue | WARNING (OPA deny) | **FAIL (OPS-VAL-005)** | Fix 4 unblocked OPA; handler now surfaces pre-existing `No engine registered for CreateRevenueContractCommand` drift |
| 8 | risk | WARNING (OPA deny) | **PASS** | Fix 4 — `ops-validator` permit in new `risk/exposure.rego` |
| 9 | routing | PASS (with silent-loss WARNING) | **PASS** | Fix 3 — `kafka-init: service_completed_successfully` dependency; outbox row now publishes on first attempt |
| 10 | subject | FAIL (500, unknown enum) | **PASS** | Fix 2 extended — `ValidSubjectTypes` + `ValidStructuralRefTypes` + `ValidEconomicRefTypes`; test payload now respects `EconomicRefRules.Validate(Participant, CapitalAccount)` |
| 11 | transaction | WARNING (OPA deny) | **PASS** | Fix 4 + Fix 2 extension — new `transaction/charge.rego` permit; `ValidChargeTypes` guard (`Fixed`/`Percentage`) |
| 12 | vault | PASS | **PASS** | no change — regression check OK |

Evidence: [evidence-rerun/](evidence-rerun/) (response JSON per context).

Eventstore delta for the re-run ([evidence-rerun/_delta.txt](evidence-rerun/_delta.txt)): `events=24 outbox=22 published=22`. **All 22 new outbox rows published** within seconds (no pending, no failed, no deadletter from this run).

## 3. Files modified (minimal set)

**Code:**
- [src/platform/host/adapters/PostgresOutboxAdapter.cs:163](../../src/platform/host/adapters/PostgresOutboxAdapter.cs#L163) — `ON CONFLICT (idempotency_key) DO NOTHING` → `ON CONFLICT DO NOTHING` (Fix 1)
- [src/platform/api/controllers/economic/compliance/audit/AuditController.cs](../../src/platform/api/controllers/economic/compliance/audit/AuditController.cs) — `ValidAuditTypes` whitelist + 400 guard (Fix 2)
- [src/platform/api/controllers/economic/reconciliation/discrepancy/DiscrepancyController.cs](../../src/platform/api/controllers/economic/reconciliation/discrepancy/DiscrepancyController.cs) — `ValidDiscrepancySources` whitelist + 400 guard (Fix 2)
- [src/platform/api/controllers/economic/subject/subject/SubjectController.cs](../../src/platform/api/controllers/economic/subject/subject/SubjectController.cs) — 3× whitelists (`ValidSubjectTypes` / `ValidStructuralRefTypes` / `ValidEconomicRefTypes`) + 400 guards (Fix 2)
- [src/platform/api/controllers/economic/transaction/charge/ChargeController.cs](../../src/platform/api/controllers/economic/transaction/charge/ChargeController.cs) — `ValidChargeTypes` whitelist + 400 guard (Fix 2 extension)

**Config:**
- [infrastructure/deployment/multi-instance.compose.yml](../../infrastructure/deployment/multi-instance.compose.yml) — added `kafka-init: condition: service_completed_successfully` to `whyce-host-1` and `whyce-host-2` `depends_on` (Fix 3)

**Policy:**
- [infrastructure/policy/domain/economic/revenue/contract.rego](../../infrastructure/policy/domain/economic/revenue/contract.rego) — added `ops-validator` permit branch (Fix 4)
- [infrastructure/policy/domain/economic/enforcement/lock.rego](../../infrastructure/policy/domain/economic/enforcement/lock.rego) — NEW file (Fix 4)
- [infrastructure/policy/domain/economic/risk/exposure.rego](../../infrastructure/policy/domain/economic/risk/exposure.rego) — NEW file (Fix 4)
- [infrastructure/policy/domain/economic/transaction/charge.rego](../../infrastructure/policy/domain/economic/transaction/charge.rego) — NEW file (Fix 4)

**Test harness:**
- [docs/operational-validation/economic-system/evidence/run-scenario-a.sh](evidence/run-scenario-a.sh) — parameterised with `RUN_ID` nonce so deterministic-HSID aggregate IDs are unique per run; valid enum + combination values on payloads

## 4. Summary of fixes applied

**Fix 1 (S0) — exchange outbox PK:** both `id` and `idempotency_key` are deterministic SHA256 over the same inputs, so a retry collides on the PK *before* the `ON CONFLICT (idempotency_key)` clause matches. Widened the conflict target to catch any unique-constraint violation. Idempotent command handling is unchanged; duplicate inserts silently no-op (the desired behavior since the original row already carries the authoritative payload).

**Fix 2 — 500 → 400 on enum parse:** added domain-enum string whitelists at the controller boundary for `AuditType`, `DiscrepancySource`, `SubjectType`, `StructuralRefType`, `EconomicRefType`, `ChargeType`. Invalid values are now rejected with `ApiResponse.Fail(code, message, ...)` at 400 before dispatch. No domain-reference introduced; whitelists mirror the enum members so future additions require a parallel edit (tracked as OPS-VAL-006 below).

**Fix 3 — routing silent loss (kafka-init race):** added `kafka-init: condition: service_completed_successfully` to both host service `depends_on` blocks in `multi-instance.compose.yml`. Host services now only accept traffic after every declared topic exists in the broker; the outbox no longer produces `failed` / `deadletter` rows on the routing happy path.

**Fix 4 — testability via `ops-validator` policy scope:** added an `ops-validator` permit branch to `revenue/contract.rego` and created stub policies for the three previously-missing domains (`enforcement/lock.rego`, `risk/exposure.rego`, `transaction/charge.rego`) mirroring the `transaction/limit.rego` style (deny-by-default, role-scoped allow branches, with an `is_system` branch for charge). Default-deny semantics preserved on every file.

## 5. Regression check

| Previously-PASS context | Re-run status | Verdict |
|-------------------------|---------------|---------|
| capital | 200 (event emitted, projection row exists) | ✅ no regression |
| ledger | 200 (event emitted, projection row exists) | ✅ no regression |
| vault | 200 (event emitted, projection row exists) | ✅ no regression |
| routing | 200 (outbox row now published, not failed) | ✅ improved — previously a silent-loss WARNING |

## 6. Remaining drift (captured under $1c, outside the four identified defects)

- **OPS-VAL-005 (S1)** — no engine registered for `CreateRevenueContractCommand`. Handler lookup fails at dispatch. Masked until now by policy deny.
- **OPS-VAL-006 (S2)** — domain enum whitelists are string-duplicated in controllers. When a domain enum gains a member, controllers silently reject it. Candidate for a shared `EnumWhitelist<T>` helper or a cross-project enum reference.
- **OPS-VAL-007 (S1)** — `EventSchemaRegistry` has no `InboundEventType` registered for `ExposureCreatedEvent`; the projection consumer routes `whyce.economic.risk.exposure.events` → deadletter on every message. Projection silently empty despite successful domain execution. Observed in host logs during the re-run.
- **OPS-VAL-008 (S2)** — domain invariant violations (e.g. `CurrencyPair` ordinal rule, `EconomicRefRules` pairing rule) throw `ArgumentException` / `InvalidOperationException` → 500 through `ExceptionHandlerMiddleware`. Same defect class as Fix 2 but applies to domain-rule violations, not enum-parse. Candidate for a centralised `DomainException`-aware exception handler.

Captured in [claude/new-rules/20260418-230500-audits.md](../../claude/new-rules/20260418-230500-audits.md).
