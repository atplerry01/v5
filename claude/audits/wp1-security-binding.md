# WP-1 Security Binding — Official Record

**Status**: PASS
**Date**: 2026-04-13
**Classification**: phase2-security
**Lock**: CANONICAL — This is the official security binding record.

---

## Summary

JWT-based identity binding fully enforced.

All requests require:

* valid JWT (signature, expiration, issuer, audience validated)
* authenticated identity (sub claim extracted as ActorId)
* authorization gate ([Authorize] on all operational controllers)

ActorId is:

* extracted from JWT (sub claim) via ICallerIdentityAccessor
* propagated through CommandContext (init-only, immutable)
* used in policy evaluation (WhyceIdEngine resolves full identity, OPA + WhycePolicy evaluate)
* embedded in event and chain evidence (EventFabric carries context through EventStore, ChainAnchor, Outbox)

System is fail-closed:

* no anonymous execution (missing token = 401)
* no fallback identity (no "system" hardcode, accessor throws if unauthenticated)
* no bypass paths (all commands route through RuntimeControlPlane middleware pipeline)

This establishes a zero-trust execution boundary.

---

## Binding Points

| Layer | Binding | Enforcement |
|---|---|---|
| HTTP Edge | JWT Bearer middleware | Invalid/missing token = 401 |
| Controller | [Authorize] attribute | Unauthenticated = rejected before dispatch |
| Dispatcher | ICallerIdentityAccessor.GetActorId() | Throws if no authenticated principal |
| Middleware 3 | ContextGuardMiddleware | Validates ActorId present on CommandContext |
| Middleware 5 | PolicyMiddleware / WhyceIdEngine | Resolves IdentityId, Roles, TrustScore from ActorId |
| Middleware 6 | AuthorizationGuardMiddleware | Verifies IdentityId set + policy decision approved |
| Fabric | EventStore + ChainAnchor + Outbox | Events carry CommandContext with real ActorId |

## Files

| File | Role |
|---|---|
| src/shared/contracts/runtime/ICallerIdentityAccessor.cs | Shared interface |
| src/platform/host/Adapters/HttpCallerIdentityAccessor.cs | HTTP adapter |
| src/platform/host/composition/infrastructure/authentication/AuthenticationInfrastructureModule.cs | JWT config |
| src/runtime/dispatcher/SystemIntentDispatcher.cs | Identity injection into CommandContext |
| src/platform/host/Program.cs | UseAuthentication + UseAuthorization |
| src/platform/api/controllers/**/TodoController.cs | [Authorize] |
| src/platform/api/controllers/**/KanbanController.cs | [Authorize] |
| src/platform/api/controllers/**/HealthController.cs | [AllowAnonymous] |

## Invariants (LOCKED)

1. **WP1-INV-01**: No operational endpoint is reachable without a valid JWT Bearer token.
2. **WP1-INV-02**: CommandContext.ActorId is never "system" for HTTP-originated commands.
3. **WP1-INV-03**: ICallerIdentityAccessor.GetActorId() throws if no authenticated identity exists.
4. **WP1-INV-04**: Health/liveness/readiness endpoints remain anonymous ([AllowAnonymous]).
5. **WP1-INV-05**: JWT signing key is externalized (environment variable), never in source control.
6. **WP1-INV-06**: 8-middleware pipeline order is unchanged by this binding.
7. **WP1-INV-07**: No determinism violations introduced (no DateTime.UtcNow, Guid.NewGuid, Random).
