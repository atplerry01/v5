# TITLE
R3.B.2 First Real Adapter — HTTP Webhook Delivery End-to-End

## CONTEXT
R3.B.1 Foundation landed 2026-04-20 12:41 UTC with seam, prohibition, NoOp test adapter. Closure requires a real provider-backed end-to-end proof per parent design §13 (toy adapter does NOT satisfy exit criteria). The sixteen §15 closure rows remain at ABSENT / PARTIAL pending a real adapter that exercises durable intent → real outbound call → provider ack shape → duplicate suppression → evidence trail → timeout classification → breaker protection.

## OBJECTIVE
Ship the HTTP Webhook Delivery adapter as the first real production `IOutboundEffectAdapter`. Prove the R3.B.1 seam end-to-end with: outbound HTTP POST, HMAC-signed request, `Idempotency-Key` header propagation, 2xx/4xx/5xx classification, timeout handling, per-provider circuit breaker wiring, and Postgres-backed queue persistence. Provider finality via `ManualOnly` strategy — R3.B.4 adds inbound webhook/poll finality surfaces.

## CONSTRAINTS
- R3.B.1 locked rules (`R-OUT-EFF-PROHIBITION-01..05`, `R-OUT-EFF-SEAM-01..03`, `R-OUT-EFF-FINALITY-01..02`, `R-OUT-EFF-IDEM-01..05`, `R-OUT-EFF-TIMEOUT-01..02`, `R-OUT-EFF-OBS-01..02`, `R-OUT-EFF-DET-01`, `R-OUT-EFF-QUEUE-01..03`, `DG-R-OUT-EFF-PLACEMENT-01`) stay intact.
- Determinism seams only (`IClock`, `IIdGenerator`, `IRandomProvider`); no `Guid.NewGuid` / `DateTime.UtcNow` / `Random` in outbound subtrees.
- Reuse R2.A.D circuit breaker pattern — per-provider breaker named `outbound.{providerId}`; breaker-open never counts against retry budget.
- D-R3B-4 LOCKED: dedicated `outbound_effect_dispatch_queue` Postgres table; no reuse of the event-fabric outbox.
- HttpClient usage confined to the new adapter file under `src/platform/host/adapters/outbound-effects/`.

## EXECUTION STEPS
1. Shared contract: `WebhookEffectPayload` record (target URL, body, optional headers).
2. Adapter: `WebhookDeliveryAdapter` with `ProviderId = "http-webhook"`, `ProviderIdempotent` shape, `ManualOnly` finality.
3. Options: `WebhookDeliveryOptions` for HMAC signing key + signature header + connect timeout defaults; register `OutboundEffectOptions` for the provider.
4. Postgres queue store: `PostgresOutboundEffectQueueStore` using `EventStoreDataSource` pool; atomic insert with `(provider_id, idempotency_key)` UNIQUE; `FOR UPDATE SKIP LOCKED` claim.
5. Relay update: resolve per-provider `ICircuitBreaker` via `ICircuitBreakerRegistry.TryGet($"outbound.{providerId}")`; wrap adapter call in `breaker.ExecuteAsync`; `CircuitBreakerOpenException` → `TransientFailed` without consuming retry budget.
6. Composition: register adapter, breaker per provider, switch queue store to Postgres when `EventStoreDataSource` is present; retain in-memory fallback for tests.
7. Tests: adapter unit (fake `HttpMessageHandler`), duplicate suppression, timeout classification, idempotency key propagation, breaker-open behavior, architecture test that `http-webhook` adapter is in the sanctioned subtree.
8. Audit sweep per $1b; update gap matrix §15.

## OUTPUT FORMAT
- Files created/modified grouped by layer.
- Provider chosen + rationale.
- Queue-store implementation status.
- Breaker/timeout wiring evidence.
- Tests added.
- §15 rows transitioning ABSENT/PARTIAL → PRESENT.
- Remaining gaps for R3.B.3–R3.B.5.
- Maturity verdict.

## VALIDATION CRITERIA
- Build clean, new tests GREEN, 0 dependency-graph violations.
- Architecture tests prove whitelist still holds.
- `WebhookDeliveryAdapter` end-to-end test proves schedule → queue → relay → adapter → Acknowledged lifecycle event → projection.
- Post-execution $1b sweep PASS.

## CLASSIFICATION
- Layer: platform/host/adapters + runtime + shared contracts + infrastructure
- Context: outbound-effect
- Domain: (N/A — cross-layer seam extension)
- Severity: S1 (closes the §15 S1 correctness frontier)
