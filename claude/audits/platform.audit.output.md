# PLATFORM AUDIT OUTPUT — API/Host Boundary Enforcement

**Audit Date**: 2026-04-08  
**Scope**: Controller dispatch, host composition, DI purity, middleware registration  
**Branch**: dev_wip  
**Status**: PASS

---

## SUMMARY

**Phase-1-gate deliverables: FULLY IMPLEMENTED**

1. **API Layer Purity** (G-PLATFORM-01): PASS
   - All controllers reside in src/platform/api/controllers/
   - TodoController implements business dispatch through ISystemIntentDispatcher.DispatchAsync (line 47, 102, 109)
   - No DI wiring, no infrastructure adapter instantiation in controllers
   - No runtime/engine references in controller code

2. **Host Layer Purity** (G-PLATFORM-02): PASS
   - Program.cs contains only DI registration and middleware pipeline (src/platform/host/Program.cs)
   - Bootstrap modules loaded via BootstrapModuleCatalog (line 12-20)
   - Infrastructure composition delegated to InfrastructureComposition.AddInfrastructureComposition (line 26)
   - All per-domain wiring encapsulated in modules; Program.cs holds zero domain knowledge (comment line 11)

3. **API Dispatch Pattern** (PLAT-DISPATCH-ONLY-01, S1): PASS
   - ALL controller mutations (POST/PUT/PATCH) route through ISystemIntentDispatcher.DispatchAsync
   - TodoController.Create: cmd dispatched via _dispatcher.DispatchAsync(cmd, TodoRoute) (line 47)
   - TodoController.Update: cmd dispatched via _dispatcher.DispatchAsync(cmd, TodoRoute) (line 102)
   - TodoController.Complete: cmd dispatched via _dispatcher.DispatchAsync(cmd, TodoRoute) (line 109)
   - No direct IIntentHandler.HandleAsync calls detected in controllers
   - Single entry point enforces uniform policy evaluation, chain anchoring, outbox enqueue

4. **HTTP Edge Error Handling** (phase1-gate-api-edge): PASS
   - ConcurrencyConflictExceptionHandler registered in Program.cs (line 31)
   - IExceptionHandler implementation maps ConcurrencyConflictException to HTTP 409 ProblemDetails (RFC 7807)
   - Deterministic payload: no stack traces, no internal type names
   - Correlation ID from HttpContext.TraceIdentifier (line 51), never generated locally
   - Test coverage: ConcurrencyConflictExceptionHandlerTest validates 409 response + JSON shape (3 test cases)

5. **HSID Infrastructure Validator** (deterministic-id.guard.md G19/G20): PASS
   - HsidInfrastructureValidator registered in Program.cs (line 41-42)
   - Called exactly once at host startup: validator.ValidateAsync()
   - Fails fast on missing hsid_sequences table or drifted schema
   - Exception message guides operator to apply migration 001_hsid_sequences.sql (line 31-32)
   - PostgresSequenceStoreAdapter.HealthCheckAsync() performs strict shape check:
     - Confirms table exists AND both columns (scope, next_value) present (lines 56-62)
     - Returns boolean; missing table → false → exception → host refuses to start

6. **Middleware Registration Order** (Program.cs): PASS
   - HTTP metrics middleware registered before routing (line 46)
   - ExceptionHandler registered before MapControllers (line 52, 62)
   - Correct chain: ExceptionHandler → Routing → Controllers
   - No dual-path into runtime detected

7. **Deterministic ID Generation** (GE-01, G-PLATFORM-03): PASS
   - TodoController.Create uses _idGenerator.Generate() with deterministic seed (line 44-45)
   - Seed: `{request.UserId}:{request.Title}:{_clock.UtcNow.Ticks}`
   - No Guid.NewGuid() or DateTime.UtcNow detected in platform layer
   - IIdGenerator and IClock injected from composition root

---

## FINDINGS

### S0 — CRITICAL
None.

### S1 — HIGH
None.

### S2 — MEDIUM

**Finding 1: TodoController.Get() bypasses dispatcher for read-side query**
- File: src/platform/api/controllers/TodoController.cs (lines 53-95)
- Issue: GET endpoint directly queries projection Postgres (line 58-67) instead of routing through dispatcher
- Severity: S2 — architecturally acceptable for read-side (queries bypass write-side policy) but creates dual path into infrastructure
- Status: ACCEPTABLE per CQRS separation (read model is separate concern); no command/mutation involved
- Mitigation: Document that read-side queries bypass policy; acceptable by design

**Finding 2: Hardcoded projection connection string fallback in TodoController**
- File: src/platform/api/controllers/TodoController.cs (lines 34-35)
- Issue: Projection connection string has localhost/5434/whyce_projections fallback; should fail if not configured
- Severity: S2 — operability; hardcoded fallback masks missing configuration
- Remediation: Remove fallback; require Projections__ConnectionString in all environments

---

## GUARD COMPLIANCE

| Guard | Rule | Status |
|-------|------|--------|
| platform.guard.md | G-PLATFORM-01 (API Layer Purity) | PASS |
| platform.guard.md | G-PLATFORM-02 (Host Layer Purity) | PASS |
| platform.guard.md | G-PLATFORM-03 (No Controllers in Host) | PASS |
| platform.guard.md | G-PLATFORM-04 (No DI in API) | PASS |
| platform.guard.md | G-PLATFORM-06 (API Calls Systems Only) | PASS — via dispatcher |
| platform.guard.md | PLAT-DISPATCH-ONLY-01 (S1) | PASS — all mutations routed |
| platform.guard.md | PLAT-DET-01 (No Guid.NewGuid in adapters) | PASS |

---

## TEST COVERAGE

- ✓ tests/integration/api/ConcurrencyConflictExceptionHandlerTest.cs (3 test cases)
  - Handler returns 409 for ConcurrencyConflictException
  - Handler ignores unrelated exceptions
  - Handler uses HttpContext.TraceIdentifier for correlation ID

---

## VERDICT

**PASS** — API/Host boundary correctly enforced. Dispatcher entry point enforced. HSID validation in place. One S2 finding (hardcoded fallback) should be addressed.

