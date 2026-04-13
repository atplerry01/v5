# Security & Policy Hardening Audit — Phase 2 Entry

**Audit Date:** 2026-04-13
**Classification:** certification / constitutional-system / security
**Scope:** Zero-trust posture, actor identity, policy enforcement, evidence logging, bot resistance, economic controls, infrastructure security
**Auditor:** Claude (Opus 4.6)
**Status:** CONDITIONAL PASS

---

## Executive Summary

The Whycespace runtime implements a **defense-in-depth, policy-gated execution pipeline** with strong internal security guarantees. No command can reach an engine without passing through identity resolution, OPA policy evaluation, constitutional policy evaluation, and multiple authorization guards. Policy decisions (allow and deny) are persisted to a dedicated audit stream and chain-anchored. The middleware pipeline is locked at composition time and cannot be reordered or bypassed.

**However**, the system currently lacks HTTP-level authentication. All API requests are dispatched with a hardcoded `ActorId = "system"` and receive the same `["user"]` role. The identity framework (WhyceId), policy framework (WHYCEPOLICY + OPA), and evidence framework (WhyceChain) are all functional and non-bypassable — but they operate on a uniform synthetic identity rather than real caller identity. Dedicated anti-bot/automation enforcement mechanisms are not yet implemented, though the architectural seams for them exist.

**Verdict:** The core admission/evidence path is **secure by construction**. Enterprise-grade authentication, role differentiation, and anti-automation hardening are **incomplete** and must be addressed in early Phase 2.

---

## A. IDENTITY & ACTOR INTEGRITY

### Q1: Can any command reach execution without a meaningful actor identity?

**NO command can reach execution without an actor identity** — but the identity is always the same.

**Evidence chain:**
1. `SystemIntentDispatcher.cs:47` sets `ActorId = "system"` for all commands
2. `ContextGuardMiddleware.cs:24` blocks empty/null ActorId — hard stop
3. `PolicyMiddleware.cs:56-61` resolves identity via `WhyceIdEngine.AuthenticateIdentity()`
4. `PolicyMiddleware.cs:63-66` blocks `IsAuthenticated == false` — hard stop
5. `AuthorizationGuardMiddleware.cs:20` blocks null/empty `IdentityId` — hard stop
6. `RuntimeControlPlane.cs:273` blocks null/empty `IdentityId` — defense-in-depth hard stop

**Finding:** Identity is **always resolved and enforced**, but resolves to the same "system" identity for every HTTP request. The framework is functional; the HTTP-level binding is missing.

**Classification: B — HARDENING GAP**

### Q2: Are anonymous, empty, or synthetic actor identities blocked consistently?

**Empty identities: YES, blocked.** `ContextGuardMiddleware` rejects empty ActorId. `WhyceIdEngine` returns `IsAuthenticated = false` for null/empty input, which PolicyMiddleware treats as a hard stop.

**Synthetic "system" identity: NOT blocked — it IS the identity.** `SystemIntentDispatcher` hardcodes `ActorId = "system"` for all requests. This passes all guards because it's a non-empty, valid string.

**Classification: B — HARDENING GAP** (system actor is valid but undifferentiated)

### Q3: Is WhyceID represented in the actual runtime/security flow?

**YES — WhyceID is functional and on the critical path.**

- `WhyceIdEngine.cs` performs identity resolution from ActorId
- `IdentityResolver.cs` maps userId → identityId deterministically
- `TrustScoreEvaluator.cs` computes trust scores
- `PolicyMiddleware.cs:70-72` writes `IdentityId`, `Roles`, `TrustScore` to CommandContext (write-once locked)

**Classification: D — NO ISSUE** (WhyceID is real, not conceptual)

### Q4: Is actor identity propagated into policy evaluation, execution context, and evidence logging?

**YES — fully propagated.**

- Policy evaluation: `PolicyMiddleware.cs:58,79` passes ActorId to OPA context
- Execution context: `CommandContext.IdentityId` is write-once locked after PolicyMiddleware
- Evidence logging: `PolicyDecisionEventFactory` stamps IdentityId on all policy decision events
- Chain anchoring: Events carry the CorrelationId that traces back through the identity-resolved context
- Tracing: `TracingMiddleware.cs:27` tags `actor.id` on the activity span

**Classification: D — NO ISSUE**

---

## B. POLICY ENFORCEMENT

### Q5: Does WHYCEPOLICY actually sit on the admission path before execution?

**YES — dual-layer policy evaluation is mandatory and non-bypassable.**

1. **OPA (external):** `OpaPolicyEvaluator.cs` sends policy requests to OPA REST API with circuit breaker, timeout, and fail-closed semantics. Failure throws `PolicyEvaluationUnavailableException` — **never silently allows**.
2. **WhycePolicyEngine (constitutional):** Evaluates safeguards (identity required, trust floor) and policy rules. Only runs if OPA allows.
3. **Pipeline position:** PolicyMiddleware is position 5 of 8 in the locked pipeline. `RuntimeControlPlaneBuilder` throws at composition time if PolicyMiddleware is missing.

**Classification: D — NO ISSUE**

### Q6: Can any route/command/workflow bypass policy by missing or malformed metadata?

**NO.** The ContextGuardMiddleware (position 3) requires non-empty CorrelationId, TenantId, ActorId, PolicyId, and AggregateId. The ValidationMiddleware (position 4) requires non-null command, non-empty CommandId and CausationId. All are enforced before PolicyMiddleware runs.

For workflow commands specifically, `SystemIntentDispatcher.cs:47-48` hardcodes `PolicyId = "whyce-policy-default"`. This cannot be overridden by the HTTP caller.

**Classification: D — NO ISSUE**

### Q7: Are policy decisions captured with enough evidence for audit and replay?

**YES — both allow and deny paths emit to a dedicated audit stream.**

- Allow: `PolicyEvaluatedEvent` with IdentityId, PolicyName, DecisionHash, ExecutionHash, PolicyVersion, CommandId, CorrelationId
- Deny (OPA): `PolicyDeniedEvent` with same fields
- Deny (Constitutional): `PolicyDeniedEvent` with same fields
- Routing: Dedicated `policy-audit-stream:{CommandId}` aggregate, separate from domain streams
- Kafka topic: `whyce.constitutional.policy.decision.events`
- DecisionHash: SHA256 deterministic (no clock, no RNG)

**Classification: D — NO ISSUE**

### Q8: Are constitutional/system-critical actions more strongly protected?

**PARTIALLY.** WhycePolicyEngine safeguards (identity required, trust floor) apply to ALL commands uniformly. OPA rego policies differentiate by domain (economic transactions require operator/admin role; sandbox operations accept user role). However, since all identities currently receive the "user" role, the economic policies would DENY transactions — which is actually MORE restrictive than intended.

**Classification: C — ACCEPTABLE TEMPORARY SEAM** (role differentiation framework exists; static assignment is bounded)

---

## C. AUTHORIZATION & EXECUTION GUARDS

### Q9: Is authorization enforced before execution, not merely at the UI edge?

**YES — authorization is enforced at 5 distinct gates within the runtime pipeline.**

| Gate | Layer | Check |
|------|-------|-------|
| PolicyMiddleware | Middleware #5 | Evaluates identity + OPA + constitutional policy |
| AuthorizationGuardMiddleware | Middleware #6 | Confirms IdentityId + PolicyDecisionAllowed + PolicyDecisionHash |
| ExecutionGuardMiddleware | Middleware #8 | Confirms PolicyDecisionAllowed + PolicyDecisionHash |
| DispatchWithPolicyGuard | Control Plane | Defense-in-depth: PolicyDecisionAllowed + IdentityId |
| Event emission boundary | Control Plane | PolicyDecisionAllowed + PolicyDecisionHash before persist |

**Classification: D — NO ISSUE**

### Q10: Are middleware/guards ordered correctly and consistently?

**YES — order is locked at composition time.**

`MiddlewareOrderGuard.cs` validates against a static `CanonicalOrder` array. `RuntimeControlPlaneBuilder` enforces that all 8 middleware are present and throws `InvalidOperationException` if any is missing. The build output is an `IReadOnlyList<IMiddleware>` — immutable after composition.

Canonical order: Tracing → Metrics → ContextGuard → Validation → Policy → AuthorizationGuard → Idempotency → ExecutionGuard

**Classification: D — NO ISSUE**

### Q11: Are idempotency, policy, and authorization cooperating correctly?

**YES.** IdempotencyMiddleware runs AFTER policy (position 7). This means:
- Denied commands are NOT cached in the idempotency store
- Only policy-approved commands are deduplicated
- Failed executions release the idempotency claim (allowing retry)
- The idempotency key is `{CommandType}:{CommandId}` — deterministic

**Classification: D — NO ISSUE**

---

## D. EVIDENCE & CHAIN INTEGRITY

### Q12: Are policy decisions, execution facts, and action outcomes logged evidentially?

**YES.** The EventFabric enforces: persist (EventStore/Postgres) → chain anchor (WhyceChain) → outbox (Kafka). Every event envelope carries:
- `ExecutionHash` (deterministic, computed from context + events)
- `PolicyHash` (from `context.PolicyDecisionHash`)
- `CorrelationId`, `CausationId`, `EventId` (deterministic)

Policy decisions are additionally logged to a dedicated audit stream via `AuditEmission`.

**Classification: D — NO ISSUE**

### Q13: Is WhyceChain anchoring actually connected to the relevant paths?

**YES.** `EventFabric.ProcessInternalAsync` calls `ChainAnchorService.AnchorAsync` after every successful event persistence. The chain uses SHA256 block hashing: `SHA256(previousHash + payloadHash + sequence)` — no timestamps, fully deterministic. The chain anchor is protected by a semaphore (PermitLimit=1) with timeout.

**Classification: D — NO ISSUE**

### Q14: Are there security-significant actions not chain/evidence visible?

**ONE GAP: HTTP-level request metadata is not captured in the evidence chain.** Since there is no HTTP authentication, request source (IP, headers, user agent) is not propagated into the CommandContext or evidence logging. Rate limiter rejections are metered but not chain-anchored (they don't reach the runtime pipeline). This is expected for the current design but means pre-admission activity is not evidentially logged.

**Classification: B — HARDENING GAP**

---

## E. BOT / AUTOMATION RESISTANCE

### Q15: Is the system truly hostile-by-default toward untrusted automation?

**PARTIALLY.** The three-tier assessment:

**1. Core admission safety (PRESENT NOW):**
- Every command requires identity resolution, policy evaluation, and authorization — non-bypassable
- OPA evaluator has circuit breaker + timeout + fail-closed semantics
- Intake rate limiter (per-tenant/per-IP concurrency ceilings with 429 + Retry-After)
- Workflow admission gate (per-workflow + per-tenant concurrency ceilings)
- Distributed execution lock (prevents concurrent duplicate execution)
- Idempotency middleware (prevents replay of same CommandId)

**2. Dedicated anti-bot/behavioral enforcement (NOT YET IMPLEMENTED):**
- No request fingerprinting
- No behavioral analysis
- No CAPTCHA or proof-of-work
- No device attestation
- No abuse scoring beyond static trust score
- No dynamic rate limiting based on behavior patterns

**3. Architectural readiness (PREPARED):**
- `TrustScoreEvaluator` exists and feeds into policy evaluation
- `EconomicContextResolver` can extract value signals for risk assessment
- Domain structures for risk/exposure/assessment exist
- Policy framework can be extended with behavioral rules
- The seams exist; the implementations do not

**Classification: B — HARDENING GAP** (core admission is sound; dedicated anti-automation is absent)

### Q16-Q17: Are behavioral enforcement seams and public-surface assumptions in code?

**Seams exist but are not activated.** Trust score evaluation is functional but assigns the same score to all identities. Economic context resolution exists but doesn't trigger risk controls. Rate limiting exists at HTTP and workflow levels. No public-surface-specific hardening is present.

**Classification: B — HARDENING GAP**

### Q18: Can automated high-frequency retries slip into execution paths?

**PARTIALLY MITIGATED.** Rate limiter (per-IP/per-tenant concurrency) bounds concurrent requests. Idempotency middleware blocks same-CommandId replay. Distributed execution lock prevents concurrent duplicate execution. However, an attacker sending structurally distinct commands at high frequency would only be bounded by the rate limiter's concurrency ceiling (GlobalConcurrency=6 by default).

**Classification: B — HARDENING GAP**

---

## F. ECONOMIC / HIGH-RISK ACTION CONTROL

### Q19: Are economic operations policy-gated distinctly?

**YES via OPA rego policies.** `infrastructure/policy/domain/economic/ledger/transaction.rego` restricts `transaction.submit` to operator/admin roles only. Since all current identities receive the "user" role, economic transactions would be **denied by OPA** — which is the correct restrictive behavior.

**Classification: D — NO ISSUE** (the policy gate works; role assignment is the gap)

### Q20: Can value movement occur without proper identity/policy/evidence?

**NO.** All commands — including any future economic commands — must pass through the full runtime pipeline (identity → policy → authorization → evidence → chain). There is no separate execution path for economic operations that could bypass this.

**Classification: D — NO ISSUE**

### Q21: Are there direct adapter or handler paths that bypass controls?

**NO.** The EventFabric is the single, non-bypassable persistence path. Engines cannot persist directly (engine.guard rule 3). Controllers route through `ISystemIntentDispatcher` → `RuntimeControlPlane` → full pipeline. There are no direct database writes from controllers.

**Classification: D — NO ISSUE**

---

## G. INFRASTRUCTURE & CONFIGURATION SECURITY

### Q22: Is infrastructure security externalized enough?

**PARTIALLY.** Credentials are externalized via `.env` files (gitignored). `appsettings.json` contains NO secrets — only operational configuration. Docker-compose uses `${POSTGRES_PASSWORD}` env var substitution. However, the `infrastructure/security/` directory is **empty** — no TLS certificates, no key management, no secrets rotation config.

**Classification: B — HARDENING GAP**

### Q23: Are there secrets/config anti-patterns?

**ONE ISSUE:** `infrastructure/deployment/.env` is committed to source with `change_me_securely` placeholder values. While these are explicitly marked as local-dev defaults, committing even placeholder `.env` files can train developers to commit real ones.

`src/platform/host/obj/verify/bin/appsettings.json` contains local dev credentials but is in gitignored `obj/` directory — not committed.

Docker-compose has inline credentials for monitoring tools (pgAdmin: admin/admin, Grafana: admin/admin, Prometheus exporter: plaintext DSN). These are local-dev-only.

**Classification: B — HARDENING GAP** (local dev credentials in committed files)

### Q24: Is infrastructure security merely empty structure?

**Security controls exist in runtime/platform/policy — not in `infrastructure/security/`.** The absence of the directory is organizational, not architectural. Real enforcement lives in:
- Runtime middleware pipeline (8 mandatory guards)
- OPA rego policies (functional, domain-specific)
- WhycePolicyEngine safeguards
- WhyceChain evidence logging
- Rate limiting (HTTP + workflow admission)

**Classification: C — ACCEPTABLE TEMPORARY SEAM** (organizational gap, not architectural)

### Q25: Are environment differences controlled safely?

**PARTIALLY.** `infrastructure/environments/` has dev/local/staging/production configs but they contain minimal content (just classification metadata). Real environment-specific configuration is expected via `.env` files. The pattern is sound but sparse.

**Classification: C — ACCEPTABLE TEMPORARY SEAM**

---

## H. INTERNAL AI / AGENT SAFETY

### Q26-Q28: Are internal AI/automation seams present and sandboxed?

**NO internal AI/agent seams exist in the runtime or engine code.** No LLM calls, no agent frameworks, no automation hooks. The architecture is prepared to enforce sandboxing via:
- Policy-gated execution (any AI agent would be a policy-evaluated actor)
- Trust score evaluation (AI agents would receive appropriate trust levels)
- Domain-specific rego policies (could restrict AI-originated commands)
- Chain evidence logging (AI actions would be auditable)

**Classification: D — NO ISSUE** (architecture prepared; no implementation needed yet)

---

## Seam Inventory

| # | File / Seam | Finding | Classification | Why It Matters | Action |
|---|------------|---------|---------------|----------------|--------|
| 1 | `SystemIntentDispatcher.cs:47` | `ActorId = "system"` hardcoded for all requests | **B — Hardening Gap** | All requests share same identity; no caller differentiation | Wire HTTP auth to extract real actor identity |
| 2 | `WhyceIdEngine.cs:44,48` | Roles hardcoded to `["user"]` for all identities | **B — Hardening Gap** | No role differentiation; OPA role-based policies are functional but receive uniform input | Implement real role resolution |
| 3 | Program.cs | No `UseAuthentication()` / `UseAuthorization()` | **B — Hardening Gap** | No HTTP-level authentication; API is open | Add authentication middleware |
| 4 | API Controllers | No `[Authorize]` attributes | **B — Hardening Gap** | All endpoints reachable without credentials | Add authentication requirements |
| 5 | `PolicyMiddleware.cs:57` | `Token: null` in AuthenticateIdentityCommand | **B — Hardening Gap** | No HTTP token extracted; identity from ActorId only | Extract auth token from HTTP request |
| 6 | `OpaPolicyEvaluator.cs:111` | `role = Roles.FirstOrDefault() ?? "anonymous"` | **C — Acceptable Seam** | OPA fallback to "anonymous" if roles empty; defensive only (roles always set by WhyceId) | Monitor; no patch needed |
| 7 | `infrastructure/security/` | Empty directory | **B — Hardening Gap** | No TLS certs, key management, or secrets rotation | Populate with enterprise security assets |
| 8 | `infrastructure/deployment/.env` | Committed with placeholder credentials | **B — Hardening Gap** | Trains developers to commit .env files | Move to .env.example only |
| 9 | Docker-compose monitoring | Inline credentials (pgAdmin, Grafana) | **C — Acceptable Seam** | Local-dev-only; not production exposure | Externalize for production |
| 10 | No anti-bot mechanisms | No behavioral enforcement, fingerprinting, or abuse detection | **B — Hardening Gap** | Bot-hostile doctrine partially implemented | Implement behavioral enforcement in Phase 2 |
| 11 | Health endpoints | Rate-limit exempt, unauthenticated | **C — Acceptable Seam** | Standard for liveness/readiness probes; does not expose sensitive data | Expected for infrastructure probes |
| 12 | Middleware pipeline | 8 mandatory guards, locked order, non-bypassable | **D — No Issue** | Core security architecture is sound | None |
| 13 | Policy evaluation | Dual-layer OPA + WhycePolicy, fail-closed | **D — No Issue** | Policy enforcement is real and functional | None |
| 14 | Evidence chain | Persist → chain → outbox, deterministic, policy-hashed | **D — No Issue** | Full audit trail for all executed commands | None |
| 15 | Write-once CommandContext | Identity, policy, HSID all locked after resolution | **D — No Issue** | Prevents downstream mutation of security context | None |
| 16 | Idempotency + execution lock | Prevents duplicate/concurrent command execution | **D — No Issue** | Replay and concurrency protection | None |

---

## Bot-Hostile Doctrine Assessment

| Tier | Status | Evidence |
|------|--------|----------|
| **Core admission safety** | **IMPLEMENTED** | Identity resolution, dual-layer policy, 5-gate authorization, intake rate limiter, workflow admission gate, distributed execution lock, idempotency |
| **Dedicated anti-bot enforcement** | **NOT IMPLEMENTED** | No behavioral analysis, fingerprinting, CAPTCHA, device attestation, abuse scoring, or dynamic rate adjustment |
| **Architectural readiness** | **PREPARED** | TrustScoreEvaluator, EconomicContextResolver, risk domain structures, extensible policy framework — seams exist for future implementation |

---

## Blocking Defects Fixed

**None.** No true execution bypass was found. The core admission/evidence path is secure by construction.

---

## Remaining Hardening Gaps (8)

| ID | Gap | Priority |
|----|-----|----------|
| HG-1 | No HTTP-level authentication (JWT/Bearer/API Key) | Early Phase 2 |
| HG-2 | Hardcoded "system" ActorId — no real caller differentiation | Early Phase 2 |
| HG-3 | Hardcoded "user" role — no role resolution from identity source | Early Phase 2 |
| HG-4 | No auth token extraction from HTTP request into WhyceId | Early Phase 2 |
| HG-5 | No dedicated anti-bot/behavioral enforcement mechanisms | Phase 2 |
| HG-6 | Empty `infrastructure/security/` directory | Phase 2 |
| HG-7 | Committed `.env` with placeholder credentials | Immediate cleanup |
| HG-8 | No HTTP request metadata (IP, user agent) in evidence chain | Phase 2 |

---

## Accepted Temporary Seams (4)

| ID | Seam | Boundary |
|----|------|----------|
| ATS-1 | OPA "anonymous" role fallback | Defensive only; roles are always set by WhyceId |
| ATS-2 | Static role assignment in WhyceIdEngine | Framework functional; static assignment is bounded |
| ATS-3 | Inline monitoring credentials in docker-compose | Local-dev-only; not production |
| ATS-4 | Empty `infrastructure/security/` directory | Controls exist in runtime/policy; organizational gap only |

---

## Final Verdict

### CONDITIONAL PASS

**Conditions for full PASS:**
1. **HG-1 through HG-4** (HTTP authentication + real identity binding) must be completed before Phase 2 exposes the API to untrusted callers
2. **HG-7** (committed `.env`) should be cleaned up immediately
3. **HG-5, HG-6, HG-8** should be addressed during Phase 2

**Rationale for CONDITIONAL PASS (not FAIL):**
- The core admission/evidence path is **secure by construction** — no command bypasses identity → policy → authorization → evidence
- Policy enforcement is **real and functional** (OPA + WhycePolicy, dual-layer, fail-closed)
- Evidence logging is **complete** (EventStore + WhyceChain + Kafka, deterministic)
- The middleware pipeline is **locked and non-bypassable** (8 mandatory guards, composition-time enforcement)
- The gaps are in the **HTTP binding layer**, not in the **security architecture** — the framework enforces security; it just receives a uniform identity

**Why not FAIL:**
- No command can execute without identity resolution + policy evaluation + authorization + chain-anchored evidence
- The "system" identity is not a bypass — it IS an identity that passes through all security gates
- Policy decisions (allow and deny) are evidentially logged
- The economic domain is actually MORE restricted than intended (OPA denies "user" role for economic operations)

---

## Files Audited

### Runtime Pipeline
- `src/runtime/control-plane/RuntimeControlPlane.cs`
- `src/runtime/middleware/pre-policy/ContextGuardMiddleware.cs`
- `src/runtime/middleware/pre-policy/ValidationMiddleware.cs`
- `src/runtime/middleware/policy/PolicyMiddleware.cs`
- `src/runtime/middleware/post-policy/AuthorizationGuardMiddleware.cs`
- `src/runtime/middleware/post-policy/IdempotencyMiddleware.cs`
- `src/runtime/middleware/execution/ExecutionGuardMiddleware.cs`
- `src/runtime/middleware/observability/TracingMiddleware.cs`
- `src/runtime/middleware/observability/MetricsMiddleware.cs`
- `src/runtime/dispatcher/RuntimeCommandDispatcher.cs`
- `src/runtime/dispatcher/SystemIntentDispatcher.cs`
- `src/runtime/dispatcher/WorkflowAdmissionGate.cs`
- `src/runtime/event-fabric/EventFabric.cs`
- `src/runtime/event-fabric/ChainAnchorService.cs`
- `src/runtime/guards/MiddlewareOrderGuard.cs`
- `src/runtime/guards/FabricInvocationGuard.cs`

### T0U Engines
- `src/engines/T0U/whyceid/engine/WhyceIdEngine.cs`
- `src/engines/T0U/whyceid/resolver/IdentityResolver.cs`
- `src/engines/T0U/whyceid/trust/TrustScoreEvaluator.cs`
- `src/engines/T0U/whycepolicy/engine/WhycePolicyEngine.cs`
- `src/engines/T0U/whycepolicy/registry/PolicyRegistry.cs`
- `src/engines/T0U/whycepolicy/safeguard/PolicySafeguard.cs`
- `src/engines/T0U/whycepolicy/PolicyDecisionEventFactory.cs`
- `src/engines/T0U/whycechain/engine/WhyceChainEngine.cs`
- `src/engines/T0U/whycechain/hashing/ChainHasher.cs`

### Platform / API
- `src/platform/host/Program.cs`
- `src/platform/host/adapters/OpaPolicyEvaluator.cs`
- `src/platform/host/appsettings.json`
- `src/platform/host/.env.example`
- `src/platform/api/controllers/operational/sandbox/todo/TodoController.cs`
- `src/platform/api/controllers/operational/sandbox/kanban/KanbanController.cs`
- `src/platform/api/controllers/platform/infrastructure/health/HealthController.cs`

### Shared Contracts
- `src/shared/contracts/runtime/CommandContext.cs`
- `src/shared/contracts/runtime/CommandResult.cs`
- `src/shared/contracts/runtime/AuditEmission.cs`
- `src/shared/contracts/infrastructure/policy/IPolicyEvaluator.cs`

### Infrastructure
- `infrastructure/policy/base/base.rego`
- `infrastructure/policy/domain/operational/sandbox/todo.rego`
- `infrastructure/policy/domain/operational/sandbox/kanban.rego`
- `infrastructure/policy/domain/economic/ledger/transaction.rego`
- `infrastructure/deployment/docker-compose.yml`
- `infrastructure/deployment/.env`
- `infrastructure/deployment/.env.example`
- `infrastructure/environments/production/environment.json`
- `infrastructure/security/` (empty)

### Guards
- All 36 guard files loaded per $1a
