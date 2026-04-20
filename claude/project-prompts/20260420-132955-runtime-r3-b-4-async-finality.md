# TITLE
R3.B.4 Async Finality + Reconciliation

## CONTEXT
R3.B.1 seam + R3.B.2 first real adapter + R3.B.3 hardening (shape-matrix correctness, backoff determinism, fail-closed payload registry). 64 R3.B tests green. Remaining runtime-depth gap: async confirmation / finality via webhook ingress and scheduled polling; automatic reconciliation-required emission on ack/finality timeouts; operator-safe reconcile command.

## OBJECTIVE
Close the async-finality and reconciliation surfaces so Acknowledged → Finalized transitions happen automatically via push (webhook) or pull (poll), and timeouts / orphans emit explicit lifecycle evidence rather than hidden retries. Preserve every locked invariant from R3.B.1–3 (Dispatched ≠ Acknowledged ≠ Finalized; reconciliation first-class; provider-operation-id carried on every transition; determinism; fail-closed payload registry).

## CONSTRAINTS
- R3.B.1/R3.B.2/R3.B.3 LOCKED rules unchanged.
- Webhook ingress dispatches canonical commands through sanctioned seams; NO direct aggregate mutation.
- Orphan callbacks emit explicit evidence (NOT silently discarded or silently mutating unrelated state).
- Finality timeout + ack timeout produce canonical lifecycle events (`OutboundEffectReconciliationRequiredEvent`), not logs.
- Polling uses the same adapter discipline (sanctioned subtree, determinism seams, factory-constructed events).
- Lifecycle factory remains sole mutation construction path.

## EXECUTION STEPS
1. Shared contracts: command records (`FinalizeOutboundEffectCommand`, `ReconcileOutboundEffectCommand`), poll result type, finality service contract, webhook-ingress request record.
2. Extend `IOutboundEffectAdapter` with optional `PollFinalityAsync` — default `NotSupportedException`; Push/Hybrid/Poll adapters override.
3. Extend `IOutboundEffectQueueStore` with `ClaimExpiredOrPollDueAsync` (rows where `ack_deadline <= now` AND `status = Dispatched`, OR `finality_deadline <= now` AND `status = Acknowledged`).
4. Runtime: `OutboundEffectFinalityService` concrete; `OutboundEffectReconciliationSweeper` (ack-timeout + finality-timeout → ReconciliationRequired); `OutboundEffectFinalityPoller` (invokes `PollFinalityAsync` for `Poll`/`Hybrid` adapters).
5. Ingress: `WebhookCallbackIngressHandler` that correlates by `(effectId, providerOperationId)`, validates signature via existing adapter options, emits Finalized or routes orphan to reconciliation evidence.
6. Extend `OutboundEffectReadModel` with `AckDeadline`, `FinalityDeadline`, `LastFinalitySource`, `LastFinalityEvidenceDigest`, and update projection handler.
7. Composition: register new services + hosted-service shells for sweeper / poller / webhook-ingress endpoint route.
8. Guards: `R-OUT-EFF-FINALITY-COMMAND-01`, `R-OUT-EFF-ORPHAN-CALLBACK-01`, `R-OUT-EFF-RECONCILE-PRECONDITION-01`, `R-OUT-EFF-TIMEOUT-SWEEP-01`.
9. Tests: webhook happy path, orphan reject, poll finality, ack-timeout → reconciliation, finality-timeout → reconciliation, reconcile strict precondition, ack ≠ final non-conflation preserved, projection exposes new fields.
10. Audit sweep + gap matrix + maturity verdict.

## OUTPUT FORMAT
- Files created/modified grouped by layer.
- Ingress/finality/poll/reconcile components added.
- Tests added.
- §15 rows moved to PRESENT.
- Remaining gap for R3.B.5.
- Maturity statement.

## VALIDATION CRITERIA
- Build clean, all tests green.
- 8 acceptance tests covering the required paths.
- Architecture test proves webhook ingress dispatches commands (no direct mutation).

## CLASSIFICATION
- Layer: runtime + platform/host + shared contracts + projections
- Context: outbound-effect async finality
- Severity: S1 (closes remaining §15 PARTIAL rows)
