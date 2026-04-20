# R5.C.2 Phase 2 — Chaos-Loop In-Memory End-to-End Proofs

Classification: operational
Context: runtime-reliability
Domain: chaos-loop-in-memory-proof

## TITLE
R5.C.2 Phase 2 — Promote select chaos loops from `cataloged` to `live_proven` via in-memory end-to-end proof tests. Canonical exception → canonical handler → canonical HTTP status → canonical Activity status + failure-reason tag, all in one bounded test.

## CONTEXT
R5.C.2 Phase 1 cataloged the 9 chaos observability loops and cross-reference-validated every link. What's still unproven: that the loop FIRES end-to-end when the canonical fault is triggered — not just that each link is declared, but that the cause actually produces the expected effect chain.

Phase 2 delivers **in-memory end-to-end proofs** for a subset of loops where the fault can be fabricated deterministically in a unit test (exception types, handler invocation, Activity wrapping). Phase 3 (deferred) extends this to full live-infrastructure chaos runs under load.

## OBJECTIVE
Deliver bounded in-memory chaos-loop proofs:
- `ChaosLoopHarness` — shared helper that starts an Activity (simulating the runtime's canonical span), invokes a fault action, runs it through the handler, and captures: HTTP status, ProblemDetails type URI, Activity status, Activity failure-reason tag
- 3 executable chaos-loop tests against the harness covering the simplest-to-fabricate canonical faults: domain-invariant-violation, concurrency-conflict, outbox-saturated
- Catalog promotion: flip those 3 loops from `cataloged` to `live_proven` in YAML + C# mirror
- Extended validator: every `live_proven` loop has an existing proof-test file on disk
- Guard rules locking the promotion ceremony

Not in scope:
- Metric-listener-based observation — `MeterListener` setup in tests is complex; the R4.A dashboard metric side of the loop is proven via R4.A's own tests. Phase 3 (live infrastructure) is the right place for live-metric observation.
- Log-scope observation — similar complexity; the R-TRACE-LOG-CORRELATION-01 contract is pinned by middleware-content tests.
- Live-infrastructure chaos runs under load — Phase 3 / R5.C.3.
- Remaining 6 loops (opa, chain-anchor-*, workflow-*, postgres-pool) — fabrication requires deeper infrastructure setup; defer.

## CONSTRAINTS
- No new metrics / handlers / alerts / spans.
- No runtime-layer code changes — Phase 2 is pure test harness.
- `ChaosLoopHarness` lives in `tests/integration/chaos/` (integration project already references all needed assemblies).
- Promoted loops MUST have an executable test file whose existence is validator-pinned.
- Demotion from `live_proven` → `cataloged` requires simultaneous test-file removal + status flip (catches silent test deletions).

## EXECUTION STEPS (as delivered)
1. Prompt stored per $2.
2. Build `ChaosLoopHarness` in `tests/integration/chaos/` — encapsulates ActivityListener + ActivitySource + DefaultHttpContext setup.
3. Write 3 chaos-loop proof tests:
   - `DomainInvariantChaosLoopTest` — throw DomainException inside a wrapping Activity, run through DomainExceptionHandler, verify 400 + type URI + Activity Error status + failure-reason tag.
   - `ConcurrencyConflictChaosLoopTest` — same pattern, 409 + ConcurrencyConflictException.
   - `OutboxSaturatedChaosLoopTest` — same pattern, 503 + OutboxSaturatedException + retry-after header.
4. Promote those 3 loops to `live_proven` in YAML + C# mirror, adding `proof_test:` paths.
5. Extend `ChaosObservabilityLoopTests` with: every `live_proven` loop has an existing proof-test file.
6. Promote new guard rule R-CHAOS-LOOP-LIVE-PROOF-01.
7. Sweep record per $1b.

## OUTPUT FORMAT
Summary: tests added, loops promoted, guards, validation, deferred.

## VALIDATION CRITERIA
- 3 new chaos-loop tests pass in tests/integration.
- 3 loops in the catalog now carry `loop_proof_status: live_proven` + a valid `proof_test:` path.
- `Every_live_proven_loop_has_existing_proof_test_file` validator passes.
- Prior 159 tests remain green.

## DEFERRED (R5.C.2 Phase 3 / R5.C.3)
- **Live-infrastructure chaos runs under load** — docker-compose up + scripted traffic + scripted fault injection + live-metric + live-alert observation. R5.C.2 Phase 3.
- **Remaining 6 chaos loops** (opa, chain-anchor-wait-timeout, chain-anchor-unavailable, workflow-saturated, workflow-timeout, postgres-pool-exhaustion) — fabrication requires infrastructure mocks deeper than the 3 covered here. Phase 3.
- **Metric-listener + log-scope capture inside the harness** — extends coverage of the in-memory loop. Phase 3.
- **Soak SLO proving** — R5.C.3.
