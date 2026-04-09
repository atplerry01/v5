# §5.6 Scenario 4 — OPA / WHYCEPOLICY Unavailability (EVIDENCE)

**Scenario:** Policy evaluator throws / is unavailable. Runtime must
fail closed with no downstream side-effects.
**Date:** 2026-04-09
**Test file:** [tests/integration/failure-recovery/PolicyEngineFailureTest.cs](../../../../../tests/integration/failure-recovery/PolicyEngineFailureTest.cs)
**Test method:** `Policy_Evaluator_Throwing_Is_Fail_Closed_With_No_Downstream_Side_Effects` — **PASSED [73 ms]**

## Refusal seam exercised
- §5.2.1 **PC-2** OPA bounded refusal — typed
  `PolicyEvaluationUnavailableException` mapped to **503 + Retry-After**
- In-process consecutive-failure circuit breaker
  (Closed/Open/HalfOpen) keyed by `IClock`

## Behavior verified
- A throwing policy evaluator surfaces as the typed refusal at the
  `IExceptionHandler` edge.
- **No downstream side-effects:** zero events appended, zero outbox
  rows enqueued, zero chain anchors taken, zero projection writes.
- The pipeline fails before the post-policy middleware chain runs —
  the canonical "fail closed" guarantee.

## Acceptance
| F1 | F2 | F3 | F4 | F5 | F6 |
|---|---|---|---|---|---|
| PASS — fail-closed | PASS | PASS — breaker auto-recovers | PASS — 503 + Retry-After | PASS — sub-100 ms | PASS |
