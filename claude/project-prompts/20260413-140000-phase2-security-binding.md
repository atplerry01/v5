# WP-1: Security Binding Completion

## TITLE
WP-1 — Replace static "system" identity with JWT Bearer authentication

## CONTEXT
Phase 2 security binding. The Whycespace platform previously used a hardcoded "system" ActorId for all command execution. This meant no real identity was bound to policy evaluation, authorization, or evidence chains. WP-1 closes this gap by introducing JWT Bearer authentication at the HTTP edge and propagating authenticated identity through the entire execution pipeline.

## CLASSIFICATION
- Classification: platform
- Context: security
- Domain: authentication

## OBJECTIVE
Replace the static "system" identity with real authenticated identity from HTTP requests. Propagate the authenticated ActorId through CommandContext, policy middleware, authorization guard, and evidence chain. Fail-closed: reject all unauthenticated requests to operational endpoints.

## CONSTRAINTS
- No determinism violations (no DateTime.UtcNow, Guid.NewGuid, Random)
- No architectural drift (no domain logic in platform, no runtime bypass)
- 8-middleware pipeline order must not change
- CommandContext write-once invariants preserved
- Signing key must not appear in source-controlled files
- Health/liveness/readiness endpoints remain anonymous

## EXECUTION STEPS

### Phase A — Implement
1. Create `ICallerIdentityAccessor` interface in shared/contracts/runtime/
2. Create `HttpCallerIdentityAccessor` adapter in platform/host/Adapters/
3. Create `AuthenticationInfrastructureModule` in platform/host/composition/infrastructure/authentication/
4. Wire authentication into InfrastructureCompositionRoot (first registration)
5. Add JwtBearer NuGet package to Host csproj
6. Add JWT config (non-secret) to appsettings.json
7. Add JWT__SigningKey to .env.example
8. Add UseAuthentication + UseAuthorization to Program.cs HTTP pipeline
9. Modify SystemIntentDispatcher to use ICallerIdentityAccessor instead of "system"
10. Add [Authorize] to TodoController and KanbanController
11. Add [AllowAnonymous] to HealthController
12. Update SwaggerExtensions with Bearer security definition

### Phase B — Verify
- Build: 0 errors, 0 warnings
- Determinism check: no violations
- Layer purity check: no violations
- Middleware order: unchanged

### Phase C — Audit
- Full audit sweep: all 38 audit definitions checked, all PASS

## OUTPUT FORMAT
- Audit output: claude/audits/security-binding-phase2-closure.audit.output.md
- Prompt: claude/project-prompts/20260413-140000-phase2-security-binding.md

## VALIDATION CRITERIA
- [x] No request reaches execution without valid JWT identity
- [x] ActorId sourced from JWT sub claim, not hardcoded
- [x] TenantId sourced from JWT tenant claim or X-Tenant-Id header
- [x] PolicyMiddleware receives real identity
- [x] Evidence chain includes authenticated ActorId
- [x] No determinism violations
- [x] No architectural drift
- [x] Middleware order intact
- [x] Health endpoints remain anonymous
- [x] Build succeeds cleanly

## STATUS
**PASS** — All criteria met. Security binding complete.
