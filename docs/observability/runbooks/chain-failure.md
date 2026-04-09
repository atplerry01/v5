# Runbook — Chain anchor failure

**STATUS:** TEMPLATE (resolution playbook is placeholder)

## Symptom

One or more of:

- `chain.anchor.hold_ms{outcome != "ok"}` rising (`Whyce.Chain`).
- `chain.anchor.wait_ms` rising (queue saturation on the single
  permit).
- API edge returning 503 with `ChainAnchorWaitTimeoutException` or
  `ChainAnchorUnavailableException`.
- Aggregator surfacing `chain_anchor_breaker_open` as a degraded
  reason.

## Linked SLO(s)

- [F-8 — Chain anchor failure rate](../slo/failure-rate-slos.md#f-8--chain-anchor-failure-rate)
- [L-2 — Chain anchor critical-section hold time](../slo/latency-slos.md#l-2--chain-anchor-critical-section-hold-time)
- [L-3 — Chain anchor wait time](../slo/latency-slos.md#l-3--chain-anchor-wait-time-queue-depth-proxy)
- [R-6 — Chain anchor failure decay](../slo/recovery-slos.md#r-6--chain-anchor-failure-decay)

## Severity guide (TEMPLATE)

| Severity | Condition | Routing |
|----------|-----------|---------|
| S0 | `TBD` (e.g. all chain anchors failing for ≥ X minutes) | `TBD` |
| S1 | `TBD` | `TBD` |
| S2 | `TBD` | `TBD` |
| S3 | `TBD` | `TBD` |

## Triage (real and actionable today)

1. **Identify the failure outcome.** The `chain.anchor.hold_ms`
   histogram is tagged by `outcome`:
   - `ok`             → no incident.
   - `engine_failed`  → the chain engine returned `IsAnchored=false`.
                        Look at `WhyceChainEngine` invocation logs.
   - `exception`      → the underlying chain store threw — usually
                        breaker open or transport. Cross-check the
                        chain store adapter logs.
2. **Distinguish "stuck" from "failing".** If `chain.anchor.wait_ms`
   is rising but `chain.anchor.hold_ms{outcome=ok}` is steady, the
   critical section is healthy but contention is up — likely an input
   spike, not an outage. If `wait_ms` AND `hold_ms{outcome != ok}` are
   both rising, the chain store is failing AND callers are queueing
   behind the failures.
3. **Understand the FR-5 invariant.** A chain failure is hard-stop:
   the event store HAS already persisted the events (Step 2 of
   `EventFabric`), but the chain anchor (Step 3) threw and the outbox
   enqueue (Step 4) NEVER ran. This means:
   - **No data loss** at the source of truth.
   - **Events are NOT visible to consumers** until a manual or
     replay-side recovery re-runs the anchor + enqueue path.
   - This is the canonical contract proven by
     [tests/integration/failure-recovery/ChainFailureTest.cs](../../../tests/integration/failure-recovery/ChainFailureTest.cs).
4. **Identify affected aggregates.** Cross-reference correlation IDs
   in chain-store error logs against the `events` table to enumerate
   the events that were persisted but not anchored. These are the
   events the recovery operation needs to replay.

## Mitigation (TEMPLATE)

- `TBD` — short-term steps:
  - Drain affected runtime instances if the chain store is regional.
  - Pause new command admission via the maintenance gate (HC-8).

## Resolution (TEMPLATE)

- `TBD` — root cause closure for the chain store.
- `TBD` — replay-side recovery procedure to anchor and enqueue events
  that landed in the source-of-truth during the outage. **This is
  not automatic** — the FR-5 contract is "hard-stop, replay later".

## Post-incident (TEMPLATE)

- `TBD` — verify:
  - `chain.anchor.hold_ms{outcome=ok}` is the dominant outcome again.
  - `chain.anchor.wait_ms` returned to baseline.
  - Recovery replay successfully anchored every event identified in
    triage step 4.
  - `outbox.depth` ticked up by exactly the recovered count and then
    drained as expected.

## References

- Code: [src/runtime/event-fabric/ChainAnchorService.cs](../../../src/runtime/event-fabric/ChainAnchorService.cs)
- Code: [src/runtime/event-fabric/EventFabric.cs](../../../src/runtime/event-fabric/EventFabric.cs)
- Code: [src/platform/host/adapters/WhyceChainPostgresAdapter.cs](../../../src/platform/host/adapters/WhyceChainPostgresAdapter.cs)
- Contract: [src/shared/contracts/infrastructure/chain/ChainAnchorUnavailableException.cs](../../../src/shared/contracts/infrastructure/chain/ChainAnchorUnavailableException.cs)
- Tests: [tests/integration/failure-recovery/ChainFailureTest.cs](../../../tests/integration/failure-recovery/ChainFailureTest.cs) (FR-5)
- SLO map: [docs/observability/slo/metric-mapping.md](../slo/metric-mapping.md)
