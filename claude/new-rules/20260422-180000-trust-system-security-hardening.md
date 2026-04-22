# New Rules Capture — Trust-System Security Hardening
Date: 2026-04-22
Captured during: Phase 2.8.19–2.8.22 implementation

---

## RULE-TS-SEC-01: CredentialHashValue Enforcement

**CLASSIFICATION:** trust-system / infrastructure
**SOURCE:** Phase 2.8.19 implementation — `CredentialHashValue` value object
**DESCRIPTION:** Credential secrets must never be stored as primitive strings at the domain boundary. The `CredentialHashValue` value object enforces minimum length ≥ 20 characters, blocking accidental plaintext storage at construction time.
**PROPOSED_RULE:** Any field on a domain value object named `*Hash*`, `*Secret*`, or `*Password*` that stores a credential secret MUST be typed as a domain value object (not `string`) that enforces a minimum-length invariant. Direct use of `string` for credential storage is an S1 violation.
**SEVERITY:** S1

**Why:** Plaintext credential storage is catastrophic. The domain layer is the last enforced boundary before persistence; a raw `string` property allows callers to bypass validation silently.
**How to apply:** Scan for `public string.*Hash\|Secret\|Password` on value objects and aggregates in `trust-system`. Any match that is not wrapped in a value-object type is a violation.

---

## RULE-TS-SEC-02: IIdentityThrottlePolicy Must Be Wired for Trust Operations

**CLASSIFICATION:** trust-system / infrastructure
**SOURCE:** Phase 2.8.19 — `IIdentityThrottlePolicy` introduction
**DESCRIPTION:** Every trust-system composition module that handles authentication-adjacent operations (registration, credential issuance, session open) MUST register an `IIdentityThrottlePolicy` implementation. The default `InMemoryIdentityThrottlePolicy` is acceptable for single-instance deployments; multi-instance MUST use a Redis-backed implementation.
**PROPOSED_RULE:** `TrustSystemCompositionRoot.RegisterServices` MUST register `IIdentityThrottlePolicy` as a singleton. Absence of this registration is an S2 violation (throttle bypass risk).
**SEVERITY:** S2

**Why:** Without throttle registration, callers can brute-force registration/authentication endpoints without any rate-limit signal.
**How to apply:** After any change to `TrustSystemCompositionRoot`, verify `services.AddSingleton<IIdentityThrottlePolicy` is present.

---

## RULE-TS-SEC-03: ITrustMetrics Must Be Injected in All Trust Engine Handlers

**CLASSIFICATION:** trust-system / observability
**SOURCE:** Phase 2.8.20 — `ITrustMetrics` instrumentation across 16 handlers
**DESCRIPTION:** Every T2E trust engine handler that mutates a trust aggregate MUST inject `ITrustMetrics` and call the appropriate record method after emitting domain events. This ensures denial, lockout, revocation, and lifecycle signals are always observable.
**PROPOSED_RULE:** Any `IEngine` implementation under `src/engines/T2E/trust/` that calls `context.EmitEvents(...)` MUST also call an `ITrustMetrics` method. Absence is an S2 observability violation.
**SEVERITY:** S2

**Why:** Silent trust operations are a security audit gap. An unobserved lockout or credential revocation is operationally indistinguishable from a non-existent event.
**How to apply:** Scan for `IEngine` classes under `src/engines/T2E/trust/` that lack `ITrustMetrics` constructor injection.

---

## RULE-TS-SEC-04: RegistrationLocked Projection Must Be Registered

**CLASSIFICATION:** trust-system / projection
**SOURCE:** Phase 2.8.19/2.8.21 — lockout projection wiring
**DESCRIPTION:** Any new domain event added to a trust aggregate that changes observable state MUST have a corresponding projection reducer, handler method, and `TrustProjectionModule.RegisterProjections` registration. Missing projection registration silently produces stale read models.
**PROPOSED_RULE:** After adding a new event to a trust aggregate, all three of these must be updated atomically: `RegistryProjectionReducer.Apply(state, TNewEventSchema)`, `RegistryProjectionHandler.HandleAsync(TNewEventSchema)`, and `TrustProjectionModule.RegisterProjections("NewEventName", handler)`. Missing any one is an S1 violation.
**SEVERITY:** S1

**Why:** A missing projection registration means the read model is never updated for that event, silently corrupting the view layer under Kafka replay.
**How to apply:** For each `sink.RegisterSchema("EventName", ...)` in a `TrustIdentity*SchemaModule`, verify a corresponding `projection.Register("EventName", handler)` exists in `TrustProjectionModule.RegisterProjections`.

---

## RULE-TS-SEC-05: Cross-Domain Handler Must Release Idempotency Claim on Failure

**CLASSIFICATION:** trust-system / resilience
**SOURCE:** Phase 2.8.18/2.8.22 — `RegistrationActivatedCrossDomainHandler`
**DESCRIPTION:** Any cross-domain handler that acquires an idempotency claim via `IIdempotencyStore.TryClaimAsync` MUST release the claim in a `catch` block if the downstream operation fails. Failure to release causes the handler to be permanently non-retryable for that event.
**PROPOSED_RULE:** In every `*CrossDomainHandler`, the pattern `try { await DispatchCrossDomainAsync(...); } catch { await _idempotencyStore.ReleaseAsync(...); throw; }` is mandatory. A cross-domain handler that acquires a claim without this release pattern is an S1 resilience violation.
**SEVERITY:** S1

**Why:** Without claim release, a transient downstream failure (BC unavailable, circuit open) permanently poisons the event — subsequent Kafka redelivery finds the claim taken and no-ops silently, leaving cross-domain state uninitialized forever.
**How to apply:** Scan for `TryClaimAsync` in `*CrossDomainHandler` files; every usage must have a paired `catch { await ReleaseAsync(...); throw; }` block immediately following the dispatch call.
