# Phase 2.8 — WhyceID Full Implementation Plan

## TITLE
Phase 2.8: WhyceID End-to-End Implementation — Full Identity, Authentication, Registration, Onboarding, Authorization, Trust, Verification, Consent, Session, Device, Graph, ServiceIdentity, and Audit across the full project

## CONTEXT
Phase 2.8 implements the full WhyceID system end to end across the Whycespace project. WhyceID is the canonical identity and access control plane for all actors — human, organizational, workforce, operator, service, device, and structural.

WhyceID is NOT a user table or a login module. It is a multi-engine, project-wide control system covering:
- identity truth for all actor types
- registration and onboarding workflows (including self-registration, invitation-based, org onboarding, operator onboarding)
- authentication, credential lifecycle, credential reset, and account recovery
- authorization context production (RBAC + ABAC)
- trust scoring as a policy input
- identity verification (document, contact, organizational, role)
- consent governance (grant, revoke, expiry)
- session management and privileged session controls
- device classification and restriction
- identity graph (actor relationships, delegated authority)
- service/machine identity and credential rotation
- platform bootstrap (first-operator, service identity provisioning)
- full audit and evidence integration with WhyceChain

Phase 2.8 follows the engine-by-engine build plan in `claude/project-topics/v2/phase-2.8b.md` (25 sections). Each engine produces a fully-wired D2-equivalent vertical slice: domain → engine → runtime → policy → persistence → messaging → projections → platform API → tests.

Classification: trust-system
Context: identity + access
Domain: all WhyceID engines

## OBJECTIVE
Implement the full WhyceID system end to end across the project. Every engine in the build plan (2.8.1–2.8.25) must reach production-ready state with:
- canonical domain aggregates, events, commands, errors, value objects
- engine handlers wired through runtime
- policy bindings for all identity actions
- persistence: event sourcing, event store, schema migrations
- messaging: Kafka command/event/retry/DLQ topics
- projections: read models for all engines
- platform API: all identity endpoints
- tests: aggregate unit, replay, middleware integration, E2E
- cross-domain integration: registration propagation to structural/economic/content systems
- observability: metrics per engine

The Registration and Onboarding Workflow layer (2.8.1a) is a mandatory addition to the build plan and must cover:
- registration intent model (person, org, operator, workforce)
- invitation-based and self-registration paths
- contact/email verification during registration
- account activation flow
- initial credential setup
- multi-step onboarding workflow state machine
- actor-type-specific onboarding tracks
- onboarding cross-domain propagation triggers

## CONSTRAINTS
- WBSM v3 architecture: all four canonical guard layers apply (constitutional, runtime, domain, infrastructure)
- Domain layer: zero external dependencies, pure DDD, D2 activation required before engine consumption
- No Guid.NewGuid(), no DateTime.UtcNow — use IIdGenerator and IClock
- All state changes emit domain events
- All commands pass WHYCEPOLICY before execution
- All events are chain-anchored, persisted, and outbox-routed
- Registration and identity are two distinct aggregates — registration state precedes identity creation; identity is created on activation, not on registration initiation
- No anonymous execution — every request must carry ActorId and WhyceID context (INV-202)
- Trust score is mandatory on the command context for all authenticated flows (INV-203)
- LayerPurity: trust-system domain uses `-system` suffix; engines, projections, systems layers use raw classification name
- Canonical nesting: `trust-system/identity/{domain}/` and `trust-system/access/{domain}/` in domain layer; mirror without `-system` in all other layers
- No cross-BC direct references — cross-domain communication via events or shared kernel only
- Platform bootstrapping section (2.8.25): must produce a non-anonymous, policy-bound, audited first-operator identity — no hardcoded super-admin bypass

## EXECUTION STEPS
1. Load all four guard files from `claude/guards/` before any code generation
2. Execute engine sections in the following order (each section is a full D2 vertical slice):
   a. 2.8.25 — Platform Bootstrap (foundation for all subsequent work)
   b. 2.8.1 — Identity Engine (identity aggregate, actor model, lifecycle)
   c. 2.8.1a — Registration and Onboarding Workflow Layer (registration → activation → credential setup → onboarding)
   d. 2.8.2 — Authentication Engine (login, MFA, credential reset, account recovery)
   e. 2.8.7 — Session Engine
   f. 2.8.3 — Authorization Engine (RBAC + ABAC)
   g. 2.8.8 — Device Engine
   h. 2.8.10 — ServiceIdentity Engine
   i. 2.8.4 — TrustScore Engine
   j. 2.8.5 — Verification Engine
   k. 2.8.6 — Consent Engine
   l. 2.8.9 — IdentityGraph Engine
   m. 2.8.11 — Audit Engine
   n. 2.8.12–2.8.17 — Policy, Runtime, Platform API, Persistence, Messaging, Projections integration layers
   o. 2.8.18 — Cross-domain integration (including registration propagation to structural/economic/content)
   p. 2.8.19–2.8.22 — Security hardening, Observability, Testing and certification, Resilience validation
   q. 2.8.23–2.8.24 — Documentation, anti-drift, completion evidence
3. After each engine section: run post-execution audit sweep per CLAUDE.md $1b
4. Capture any new guard errors or drift rules to `/claude/new-rules/` per CLAUDE.md $1c
5. On completion: produce evidence pack under `claude/project-topics/v2/phase-2.8-evidence.md`

## OUTPUT FORMAT
For each engine section:
- Domain layer: aggregate, entity, value objects, events, errors, specifications, services
- Engine layer: command handlers, event handlers, engine integration tests
- Runtime layer: middleware wiring, policy bindings, context propagation
- Persistence layer: event stream definitions, schema migrations, replay tests
- Messaging layer: Kafka topic declarations, outbox integration, DLQ configuration
- Projection layer: read models, projection handlers, catch-up validation
- Platform API layer: controller endpoints, DTOs, route mapping
- Test layer: aggregate unit tests, replay tests, integration tests, E2E tests

For the Registration and Onboarding section (2.8.1a), additionally produce:
- Registration API endpoints (initiate, verify contact, activate, onboarding steps)
- Onboarding state machine implementation
- Cross-domain propagation event publishing (activation → structural + economic + content)

## VALIDATION CRITERIA
- Every WhyceID engine is at D2 or equivalent activation level
- Registration → Activation → Credential Setup → Onboarding E2E flow passes
- Forgot-password and account-recovery flows pass
- Invitation-based registration flow passes
- Organization onboarding and operator onboarding flows pass
- Platform bootstrap is idempotent and produces no anonymous actors
- All aggregate invariants enforce no-duplicate-identity
- Trust score is present and policy-evaluated on every authenticated command
- Authorization deny-by-default is verified end to end
- All Kafka topics are declared in create-topics.sh
- All events carry canonical metadata (EventId, CorrelationId, CausationId, ExecutionHash, PolicyDecisionHash)
- All projections survive replay without data loss (INV-REPLAY-LOSSLESS-VALUEOBJECT-01)
- Registration propagation to structural-system, economic-system, content-system is verified
- Security hardening: no plaintext credentials, no enumerable actor IDs, lockout/throttle in place
- Completion evidence produced and stored in phase-2.8-evidence.md
