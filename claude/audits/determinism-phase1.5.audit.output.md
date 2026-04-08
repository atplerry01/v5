# DETERMINISM AUDIT — PHASE 1.5 OUTPUT

**Date:** 2026-04-08
**Sweep ID:** 20260408-132840
**Scope:** src/ (excluding tests/)
**Verdict:** **PASS** (0 × S0/S1/S2)

---

## RULE-BY-RULE RESULT

| # | Rule | Status |
|---|---|---|
| 1 | DateTime.UtcNow / .Now | PASS — only in `SystemClock.cs:7` (sole IClock impl, allowed) |
| 2 | Guid.NewGuid() | PASS — only in XML doc comments |
| 3 | Random / RNG | PASS — zero hits |
| 4 | Environment.TickCount / Stopwatch | PASS — zero hits |
| 5 | Wall-clock entropy in IIdGenerator seeds | PASS — all 3 prior S1s fixed by 07daf4c; invariant test in WbsmArchitectureTests.cs:56 prevents regression |
| 6 | Static mutable state | PASS — all static fields are readonly/immutable (Meter instances) |
| 7 | Thread.Sleep / Task.Delay in domain/engine | PASS — only in `src/platform/host/adapters/{GenericKafkaProjectionConsumerWorker,KafkaOutboxPublisher}.cs` (infra adapter, allowed) |
| 8 | File / Directory / Path APIs | PASS — zero hits in src/ |
| 9 | CultureInfo.CurrentCulture | PASS — zero hits |
| 10 | Unordered iteration | PASS — collections explicitly OrderBy'd in domain |

## HSID v2.1 GUARD VERIFICATION (G1–G20)
All 20 rules from deterministic-id.guard.md pass. Single engine, no randomness, topology required, sequence bounded, domain purity intact, single stamp point in `RuntimeControlPlane.StampHsidAsync()`.

## RESOLVED PRIOR S1 FINDINGS (commit 07daf4c)
- TodoController.cs:45 — seed no longer includes `_clock.UtcNow.Ticks`
- SystemIntentDispatcher.cs:32-35 — seeds derived from command signature only
- WorkflowDispatcher.cs (systems) — seed = `$"workflow:{name}:{payloadSignature}"`

## NEW COMMITS REVIEWED
- **d660848** (`phase1.6-S1.2: enforce lifecycle factory for resume transition`) — pure refactor, no determinism impact.

## VERDICT
Codebase is determinism-compliant and ready for Phase 2 lock condition enforcement.
