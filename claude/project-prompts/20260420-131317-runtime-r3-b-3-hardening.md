# TITLE
R3.B.3 Outbound-Effect Hardening

## CONTEXT
R3.B.1 seam + R3.B.2 WebhookDeliveryAdapter are shipped with 40 tests green and 8/10 §15 rows PRESENT. Hardening pass before widening provider surface: per-shape retry correctness, payload-registry-based queue deserialization (fail-closed), deterministic backoff pinned by architecture test, replay-equivalence for outbound-effect aggregate stream.

## OBJECTIVE
Harden what R3.B.2 proved. Lock retry classification per adapter shape; route ambiguous results on at-most-once shapes to reconciliation (not retry exhaustion); make the queue payload deserializer consult `IPayloadTypeRegistry` with fail-closed semantics; pin the relay's backoff formula as replay-deterministic via architecture test.

## CONSTRAINTS
- R3.B.1/R3.B.2 LOCKED rules unchanged.
- D-R3B1-6 payload-type-registry is authoritative; option (a) fail-closed on unknown types.
- R-OUT-EFF-IDEM-05 already prescribes the Ambiguous + AtMostOnce → ReconciliationRequired behavior; R3.B.3 wires the classification path end-to-end.
- Determinism seams only.

## EXECUTION STEPS
1. Relay: Ambiguous classification routing — `DispatchFailedPreAcceptance(Ambiguous)` on `AtMostOnceRequired` / `CompensatableOnly` shapes transitions to `ReconciliationRequired` with cause `DispatchAmbiguous`, not `RetryExhausted`.
2. Relay: backoff formula promoted to a standalone pure static helper so it can be tested independently; architecture test pins determinism (no clock/random reads).
3. Postgres queue store: inject `IPayloadTypeRegistry`; serialize with registered short name; deserialize strict via `Resolve`; duplicate-key exception preserved.
4. Tests: per-shape behavior matrix (Transient / Terminal / Ambiguous × ProviderIdempotent / NaturalKeyIdempotent / AtMostOnceRequired / CompensatableOnly); payload round-trip through registry; relay backoff determinism; replay-equivalence.
5. Guards: promote `R-OUT-EFF-AMBIGUOUS-ATMOSTONCE-01`, `R-OUT-EFF-BACKOFF-DET-01`, `R-OUT-EFF-QUEUE-PAYLOAD-REGISTRY-01`.
6. Audit sweep + gap-matrix update.

## OUTPUT FORMAT
- Files created/modified grouped by layer.
- Retry-shape matrix proof.
- Payload-registry integration evidence.
- Backoff-determinism architecture-test evidence.
- §15 rows moved.
- Remaining gaps for R3.B.4–R3.B.5.
- Maturity verdict.

## VALIDATION CRITERIA
- Build clean. All tests green. 0 dependency-graph violations.
- Architecture tests prove backoff-formula determinism + payload-registry usage.
- Per-shape behavior matrix proven by ≥ 4 targeted tests.

## CLASSIFICATION
- Layer: runtime + platform/host/adapters + shared contracts
- Context: outbound-effect hardening
- Severity: S1 (correctness hardening of a just-shipped S1 surface)
