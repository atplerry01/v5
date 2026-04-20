# R3.B.2 First Real Adapter — Post-Execution Audit Sweep

**Prompt:** [20260420-124801-runtime-r3-b-2-first-real-adapter.md](../../project-prompts/20260420-124801-runtime-r3-b-2-first-real-adapter.md)
**Design authority:** [20260420-100946-r3-b-design.md](../../project-topics/v2b/closure/20260420-100946-r3-b-design.md) §3, §6, §8 · R3.B.1 Foundation sweep 20260420-124147.
**Sweep date:** 2026-04-20 13:03 UTC

---

## 1. Guard Validation ($1a)

Guards loaded fresh. R-OUT-EFF-PROHIBITION-01 whitelist extended to cover the
R3.B.2 composition site that constructs the `HttpClient` passed into the
sanctioned adapter — exactly parallel to the OPA composition-exception
precedent. No new rules promoted; existing rule language amended.

## 2. Build Status

- All six production assemblies (Domain, Shared, Engines, Runtime, Projections, Host) build clean.
- Integration test assembly builds clean.
- Unit test assembly builds clean (pre-existing CS0162 warning in `WbsmArchitectureTests.cs:1540` unchanged, unrelated to R3.B).

## 3. Test Status

### New tests shipped with R3.B.2
- `tests/integration/integration-system/outbound-effect/WebhookDeliveryAdapterTests.cs` — **11 tests** covering: Acknowledged with digest-derived op-id, header-based op-id preference, Idempotency-Key propagation, HMAC signature stability across retries, Transient on 5xx with Retry-After, Terminal on 4xx (except 408/429), Transient on 429, Terminal on malformed URL, Terminal on payload-type mismatch, Transient on HttpRequestException, OCE propagation for relay classification. All GREEN.
- `tests/integration/integration-system/outbound-effect/OutboundEffectRelayBreakerTests.cs` — **3 tests** covering: breaker-open short-circuit preserves attempt count + emits no lifecycle events + reschedules at `RetryAfter`; relay classifies adapter OCE as `dispatch_timeout` (retryable) with `DispatchFailed + RetryAttempted` events emitted; happy-path without breaker records `Dispatched + Acknowledged` events. All GREEN.
- `tests/unit/architecture/OutboundEffectProhibitionTests.cs` — **2 new tests** added (on top of R3.B.1's 6): `Registered_Adapters_Live_In_Sanctioned_Subtree` + `WebhookDeliveryAdapter_Is_Present_And_Sanctioned`. All GREEN.

### R3.B.1 + R3.B.2 combined totals
- Unit suite (outbound-effect scope): **26 tests GREEN** (11 aggregate + 7 factory + 8 architecture).
- Integration suite (outbound-effect scope): **14 tests GREEN** (11 adapter + 3 relay-breaker).
- **40 total outbound-effect tests GREEN.**

## 4. Dependency Graph

No new forbidden edges:
- `WebhookDeliveryAdapter` lives under `src/platform/host/adapters/outbound-effects/` — sanctioned.
- `PostgresOutboundEffectQueueStore` consumes the existing `EventStoreDataSource` pool (same pattern as `PostgresDeadLetterStore`).
- Runtime `OutboundEffectRelay` now takes an optional `ICircuitBreakerRegistry` — reuses the R2.A.D registry; no new cross-layer imports.
- Composition module references `Whycespace.Runtime.Resilience.DeterministicCircuitBreaker` from Host layer — matches the OPA composition precedent.

## 5. Architectural Invariants Verified

- **R-OUT-EFF-PROHIBITION-01 / 03 / 05** — all green (architecture tests).
- **R-OUT-EFF-SEAM-01 / 02 / 03** — all green. `WebhookDeliveryAdapter` is a stateless class (no mutable instance fields); the adapter is registered singleton.
- **R-OUT-EFF-DET-01** — no `Guid.NewGuid` / `DateTime.UtcNow` / `Random` in the adapter, queue store, or relay additions. Signature HMAC uses deterministic `(signing_key, body)` inputs.
- **R-OUT-EFF-IDEM-03** (provider-key propagation) — adapter sets `Idempotency-Key` header to the effect's `OutboundIdempotencyKey.Value` on every attempt; unit test `Idempotency_key_is_propagated_on_Idempotency_Key_header` pins it.
- **R-OUT-EFF-IDEM-04** (stable key across retries) — relay reuses the queue row's `IdempotencyKey` unchanged; adapter HMAC test `Hmac_signature_header_is_stable_across_retries_for_same_body` pins determinism.
- **R-OUT-EFF-FINALITY-01 / 02** — adapter returns one of the six canonical variants exactly; `Acknowledged` carries non-null `ProviderOperationIdentity`.
- **R-OUT-EFF-QUEUE-01 / 03** — Postgres store uses `FOR UPDATE SKIP LOCKED` claim; unique `(provider_id, idempotency_key)` constraint surfaces as `OutboundEffectDuplicateKeyException` on race.
- **Parent design §6.2 breaker discipline** — breaker-open short-circuits adapter call AND does NOT consume retry budget; `BreakerOpen_short_circuits_adapter_and_preserves_attempt_count` pins both invariants.

## 6. Drift Rules Captured ($1c)

None. The R3.B.2 landing stays within the R3.B ratified design envelope; the only guard-language amendment is the whitelist extension for the composition site (parallel precedent already exists for OPA).

## 7. Anti-Drift ($5)

- No architecture changes outside the ratified R3.B design.
- No new middleware stages.
- No schema drift (the 11 lifecycle events from R3.B.1 remain the authoritative set).
- Per-provider circuit breaker name format (`outbound.{providerId}`) matches parent design §6.2 exactly.

## 8. Sweep Result

**PASS.** R3.B.2 lands with:
- 16 new tests passing (11 integration adapter + 3 integration relay-breaker + 2 architecture tests).
- 0 R3.B.2-introduced regressions.
- Build clean across all assemblies.
- Postgres queue store concrete in place with `FOR UPDATE SKIP LOCKED` claim semantics.
- First real `IOutboundEffectAdapter` (`WebhookDeliveryAdapter`) shipped and sanctioned.
- Per-provider `ICircuitBreaker` wired through the relay with correct retry-budget semantics.

## 9. Next Checkpoints

- R3.B.3 — retry classification hardening (provider `Retry-After` precedence, richer classification per-shape), deterministic retry-attempt evidence events in replay-equivalence tests.
- R3.B.4 — webhook ingress (inbound finality callback), scheduled finality poller, reconciliation-required operator command + surface.
- R3.B.5 — `OutboundEffectCompensationRequestedEvent` emission from `Finalized(BusinessFailed)` / `RetryExhausted` paths; example compensation workflow hookup; §15 S1 rows fully close.
