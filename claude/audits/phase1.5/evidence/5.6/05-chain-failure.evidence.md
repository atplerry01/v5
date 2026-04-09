# §5.6 Scenario 5 — Chain Store Failure (EVIDENCE)

**Scenario:** WhyceChain anchor write fails / chain store is degraded.
**Date:** 2026-04-09
**Test file:** [tests/integration/failure-recovery/ChainFailureTest.cs](../../../../../tests/integration/failure-recovery/ChainFailureTest.cs)
**Test method:** `Chain_Anchor_Failure_Persists_Event_But_Skips_Outbox_Enqueue` — **PASSED [136 ms]**

## Refusal seams exercised
- §5.2.3 **TC-2** `ChainAnchor.WaitTimeoutMs=5000` — typed
  `ChainAnchorWaitTimeoutException` → 503 + Retry-After
- §5.2.3 **TC-3** chain-store I/O cancellation + Closed/Open/HalfOpen
  breaker keyed by `IClock` — typed `ChainAnchorUnavailableException`
  → 503 + Retry-After
- §5.2.1 **PC-5** `chain.anchor.wait_ms` / `chain.anchor.hold_ms`
  histograms on `Whyce.Chain` meter

## Behavior verified
- When the chain anchor write fails, the **event is persisted** (the
  event store transaction commits) but the **outbox enqueue is
  skipped**, preserving the canonical contract that no event leaves
  the runtime without a chain receipt.
- The state is consistent and recoverable: a follow-up retry can
  re-anchor and enqueue without violating idempotency.
- The TC-3 breaker counts the failure; caller-driven cancellation
  does not advance the breaker (verified by §5.2.3 TC-3 closure).

## Acceptance
| F1 | F2 | F3 | F4 | F5 | F6 |
|---|---|---|---|---|---|
| PASS — event persisted | PASS — outbox skip prevents publish-without-anchor | PASS — breaker auto-recovers | PASS — 503 + Retry-After | PASS — sub-second | PASS |
