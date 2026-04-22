# New Rules Capture — Trust-System Bootstrap (2.8.25)
Date: 2026-04-22
Captured during: Phase 2.8.25 implementation

---

## RULE-TS-BOOT-01: PlatformBootstrapService Must Be Idempotent via Event Store Check

**CLASSIFICATION:** trust-system / bootstrap
**SOURCE:** Phase 2.8.25 — `PlatformBootstrapService`
**DESCRIPTION:** The platform bootstrap service that creates the first-operator identity MUST check `IEventStore.LoadEventsAsync(registryId)` before dispatching `InitiateRegistrationCommand`. If events already exist, dispatch is skipped. Without this check, every host restart would attempt a duplicate dispatch, which the event store catches at the UNIQUE constraint level (Layer 3 idempotency), producing an untyped `ConcurrencyConflictException` on startup rather than a clean skip.
**PROPOSED_RULE:** Any `IHostedService` that dispatches a lifecycle-init command at startup MUST probe the event store first and skip dispatch if events exist. The canonical seed for the deterministic ID MUST include all discriminating fields (e.g. `"platform:bootstrap:first-operator:{email}:{type}"`).
**SEVERITY:** S1

**Why:** Without the event-store check, every restart logs a noisy failure or conflict. Defense-in-depth (Layer 2) per INV-IDEMPOTENT-LIFECYCLE-INIT-01 requires the persistent probe.
**How to apply:** Any new bootstrap `IHostedService` that dispatches commands: verify `IEventStore.LoadEventsAsync` is called first. Missing probe = S1.

---

## RULE-TS-BOOT-02: Bootstrap Must Use SystemIdentityScope — No Anonymous Startup Dispatch

**CLASSIFICATION:** trust-system / bootstrap / security
**SOURCE:** Phase 2.8.25 — INV-202 compliance in `PlatformBootstrapService`
**DESCRIPTION:** Any `IHostedService` that dispatches commands at host startup has no HTTP context. Without `SystemIdentityScope.Begin(...)`, `HttpCallerIdentityAccessor.GetActorId()` throws "No HTTP context available" (WP-1 fail-closed). Bootstrap services MUST enter a named system scope before dispatch.
**PROPOSED_RULE:** Every startup `IHostedService` that calls `ISystemIntentDispatcher.DispatchAsync` or `DispatchSystemAsync` MUST wrap the dispatch call in `using var scope = SystemIdentityScope.Begin("system/{service-name}", ...)`. Absence of a `SystemIdentityScope` in a non-HTTP dispatch path is an S1 INV-202 violation.
**SEVERITY:** S1

**Why:** WP-1 is fail-closed. Without the scope, the host throws on startup, the `StartAsync` propagates the exception, and the host fails to start. This has happened and must not be repeated.
**How to apply:** Search for `DispatchAsync\|DispatchSystemAsync` calls in `IHostedService` implementations under `src/runtime/` and `src/platform/host/adapters/`. Any call not wrapped in `SystemIdentityScope.Begin` is a violation.

---

## RULE-TS-INFRA-01: SystemIdentityScope Belongs in the Runtime Layer

**CLASSIFICATION:** infrastructure / runtime
**SOURCE:** Phase 2.8.25 — moved `SystemIdentityScope` from `src/platform/host/adapters/` to `src/runtime/security/`
**DESCRIPTION:** `SystemIdentityScope` is an AsyncLocal context carrier used by runtime services to declare system identity for non-HTTP dispatch. Its consumers include handlers and services in `src/runtime/`, not just `src/platform/`. Keeping it in `src/platform/host/adapters/` made it inaccessible to the runtime layer and unit tests.
**PROPOSED_RULE:** `SystemIdentityScope` (and any future `*IdentityScope` carrier class) MUST live in `src/runtime/security/` with namespace `Whycespace.Runtime.Security`. The platform adapters layer imports from runtime security, not the reverse.
**SEVERITY:** S2

**Why:** When a runtime-layer service (like `PlatformBootstrapService`) needs to enter a scope, it cannot reference the platform/host layer — that violates layer purity. Runtime security concerns belong in the runtime layer.
**How to apply:** Any new identity-scope class added to `src/platform/host/adapters/` is a violation. Check that new scope classes are in `src/runtime/security/`.
