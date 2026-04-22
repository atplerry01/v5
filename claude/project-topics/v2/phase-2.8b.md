Proceeding with the **WhyceID engine-by-engine build plan** for **Phase 2.8**.

This is the best next artifact because WhyceID is already canonically defined as a full multi-engine system. So the implementation should be sequenced by engine, then tied back into runtime, policy, persistence, messaging, projections, and platform API.

Use this as the working plan.

**Phase 2.8 — WhyceID Engine-by-Engine Build Plan**

**2.8.0 Foundation and control topics**

* WhyceID canonical scope lock
* WhyceID implementation boundary confirmation
* full engine inventory confirmation
* project-wide adoption surface mapping
* dependency map for WhyceID engines
* identity data classification rules
* identity security baseline
* identity event catalog baseline
* identity policy catalog baseline
* completion-gate definition for Phase 2.8

**2.8.1 Identity Engine**
Purpose:

* establish the authoritative identity root for all actors in the project

Implementation topics:

* identity aggregate implementation
* actor classification model
* person identity model
* organization identity model
* workforce identity model
* operator identity model
* system identity model
* identity lifecycle states
* identity creation/update/suspend/restore flows
* canonical identifier rules
* uniqueness invariants
* identity events
* identity commands
* identity queries
* identity persistence model
* identity projections
* identity API surface

Completion proof:

* deterministic identity creation
* no duplicate identity creation
* replay-safe rehydration
* canonical read model accuracy

**2.8.1a Registration and Onboarding Workflow Layer**
Purpose:

* govern the full lifecycle from first registration intent to a fully activated, onboarded identity across all actor types
* this is a distinct workflow layer — registration is not the same as identity creation; it is the multi-step process that precedes and drives identity creation

Implementation topics — registration model:

* registration intent aggregate/state model
* registration type classification: person, organization, operator, workforce, service
* invitation-based registration model (invite token, inviter linkage, expiry)
* self-registration model (unauthenticated inbound)
* registration initiation flow
* registration data collection model (what is required per actor type)
* registration duplicate-check invariant (ties to uniqueness in Identity Engine)
* registration failure handling

Implementation topics — verification during registration:

* contact verification step (email/phone OTP during registration — distinct from Verification Engine flows)
* registration-time document verification trigger (where required by actor type)
* verification gating rules (what must pass before registration proceeds)
* verification failure and retry model

Implementation topics — activation:

* account activation model (separate state from identity creation)
* activation token issuance and expiry
* activation confirmation flow
* activation failure/retry rules
* post-activation state transitions

Implementation topics — initial credential setup:

* initial credential issuance (first-time password/passkey setup post-activation)
* credential setup enforced before session issuance
* credential setup failure and retry rules
* initial MFA enrollment (required vs optional per actor type)

Implementation topics — onboarding workflow:

* onboarding workflow state machine model
* onboarding track routing (which onboarding track applies per actor type and context)
* multi-step onboarding step model
* onboarding completion markers
* onboarding skippable vs required steps
* onboarding progress persistence
* onboarding timeout and re-entry rules

Implementation topics — actor-type-specific onboarding:

* person/user onboarding track
* organization onboarding track (legal entity setup, admin designation)
* operator onboarding track (admin/platform-operator being added by another operator)
* workforce onboarding track (employees/contractors being assigned to a structure)

Implementation topics — registration events, commands, queries, API:

* registration events: `RegistrationInitiatedEvent`, `RegistrationVerifiedEvent`, `AccountActivatedEvent`, `OnboardingStartedEvent`, `OnboardingCompletedEvent`
* registration commands
* registration queries
* registration persistence model (separate from identity aggregate — registration state is pre-identity)
* registration projections
* registration API endpoints (self-register, invite-accept, activation, onboarding steps)

Completion proof:

* end-to-end person registration → activation → credential setup → onboarding
* organization registration path completeness
* operator invite-based onboarding path completeness
* duplicate registration rejection
* expired activation token handling
* onboarding progress survives restart (replay-safe)

---

**2.8.2 Authentication Engine**
Purpose:

* prove actor identity at access time

Implementation topics:

* authentication aggregate/state model
* credential model
* credential issuance/update/revoke flows
* initial credential setup (first-time, post-registration — links back to 2.8.1a)
* login flow
* challenge flow
* multi-factor flow
* token issuance model
* refresh/revalidation model
* failed authentication handling
* lockout/throttle rules
* privileged authentication rules
* forgot-password / credential reset flow (initiated without an active session)
* account recovery flow (when all credentials and MFA are lost — escalated identity recovery path)
* magic-link / passwordless authentication model (if in scope)
* authentication events
* authentication commands
* authentication queries
* auth persistence model
* auth API surface

Completion proof:

* successful auth flow
* invalid credential rejection
* MFA path correctness
* replay-safe auth events
* session handoff correctness
* forgot-password reset round-trip correctness
* account recovery path correctness

**2.8.3 Authorization Engine**
Purpose:

* determine whether an authenticated actor may perform a requested action

Implementation topics:

* RBAC model
* ABAC model
* role registry
* permission registry
* entitlement linkage
* privilege tier model
* restricted action model
* delegated authority model
* authorization decision inputs
* identity-to-policy input mapping
* authorization events where applicable
* authorization queries
* authorization read models
* authorization API/admin surface

Completion proof:

* role-based decision correctness
* attribute-based decision correctness
* deny-by-default correctness
* project-wide authorization propagation

**2.8.4 TrustScore Engine**
Purpose:

* maintain trust-aware identity posture for policy and runtime decisions

Implementation topics:

* trust score aggregate/state model
* trust signal ingestion model
* trust calculation rules
* trust tier model
* trust raise/lower transitions
* trust degradation triggers
* trust recovery paths
* trust restriction thresholds
* trust-to-policy input mapping
* trust events
* trust projections
* trust query surface
* trust admin controls

Completion proof:

* trust score updates deterministically
* policy receives correct trust context
* threshold crossings produce expected effects
* audit trail for trust changes is complete

**2.8.5 Verification Engine**
Purpose:

* prove claims about an identity

Implementation topics:

* verification aggregate/state model
* verification type taxonomy
* identity verification flow
* contact verification flow
* document verification flow
* organizational verification flow
* role/authority verification flow
* verification expiry rules
* reverification rules
* verification failure handling
* verification events
* verification queries
* verification evidence storage linkage
* verification API surface

Completion proof:

* verification lifecycle correctness
* expired verification handling
* reverification path correctness
* evidence linkage integrity

**2.8.6 Consent Engine**
Purpose:

* govern permitted uses of identity-linked data and actions

Implementation topics:

* consent aggregate/state model
* consent purpose taxonomy
* consent scope model
* consent grant flow
* consent revoke flow
* consent expiry rules
* consent renewal rules
* consent evidence model
* consent lookup/query model
* consent-to-policy integration
* consent events
* consent projections
* consent API surface

Completion proof:

* grant/revoke correctness
* expiry handling correctness
* policy receives correct consent state
* evidence trail is complete

**2.8.7 Session Engine**
Purpose:

* manage continuity of authenticated and privileged access

Implementation topics:

* session aggregate/state model
* session issuance
* session renewal
* session expiry
* session revocation
* concurrent session policy
* privileged session model
* step-up session model
* session anomaly detection inputs
* session-device linkage
* session events
* session projections
* session query/admin surface
* session API surface

Completion proof:

* valid session continuity
* revoked session denial
* expiry correctness
* privileged session controls work correctly

**2.8.8 Device Engine**
Purpose:

* classify and govern devices interacting with Whycespace

Implementation topics:

* device identity aggregate/state model
* device registration
* device binding to identity
* trusted/untrusted device classification
* device verification flow
* device risk model
* device restriction model
* device revoke flow
* device anomaly signals
* device events
* device projections
* device query surface
* device API/admin surface

Completion proof:

* trusted device path works correctly
* untrusted/revoked device restrictions work
* device-identity linkage remains consistent
* device risk signals feed policy correctly

**2.8.9 IdentityGraph Engine**
Purpose:

* maintain the canonical relationship graph across identities, roles, structures, and services

Implementation topics:

* identity graph model
* actor-to-actor relationship model
* person-to-organization relationships
* person-to-structure relationships
* operator-to-role relationships
* service-to-system relationships
* device-to-identity relationships
* delegated authority relationships
* graph invariants
* graph mutation rules
* graph query model
* graph projections
* graph API/admin surface

Completion proof:

* relationship integrity
* no invalid graph links
* delegated authority resolution correctness
* graph query correctness

**2.8.10 ServiceIdentity Engine**
Purpose:

* provide identity and access control for machines, services, workloads, and internal actors

Implementation topics:

* service identity aggregate/state model
* service registration
* service credential lifecycle
* service authentication model
* service authorization scope model
* service trust classification
* workload identity model
* credential rotation rules
* revocation/suspension rules
* service events
* service projections
* service query/admin surface
* service integration with runtime/background workers

Completion proof:

* service registration correctness
* service auth correctness
* credential rotation correctness
* suspended/revoked service denial correctness

**2.8.11 Audit Engine**
Purpose:

* produce full identity evidence across all WhyceID actions

Implementation topics:

* identity audit record model
* audit event taxonomy
* auth event audit model
* authorization audit model
* trust change audit model
* verification evidence audit model
* consent evidence audit model
* session/device audit model
* service identity audit model
* policy decision linkage
* WhyceChain evidence linkage
* audit projections
* audit query surface

Completion proof:

* every critical action emits auditable evidence
* policy and identity decisions are linked
* chain linkage is consistent
* audit read model is complete and queryable

**2.8.12 Policy integration layer**

* WhyceID policy ID catalog
* authentication policy packages
* authorization policy packages
* trust threshold policy packages
* verification requirement policy packages
* consent requirement policy packages
* session/device restriction policies
* service identity policies
* privileged action policies
* simulation coverage for identity policy paths

**2.8.13 Runtime integration layer**

* caller identity accessor implementation
* request identity context propagation
* system identity scope implementation
* background worker identity handling
* middleware ordering alignment
* policy middleware context enrichment
* authorization guard integration
* execution guard identity restrictions
* correlation plus identity trace continuity
* deny-by-default runtime behavior

**2.8.14 Platform API integration layer**

* public identity endpoints
* operator/admin endpoints
* verification endpoints
* consent endpoints
* session endpoints
* device endpoints
* service identity endpoints
* trust/restriction endpoints where allowed
* graph/relationship endpoints where allowed
* canonical route alignment

**2.8.15 Persistence and event sourcing layer**

* event stream definitions for each engine
* event versioning rules
* deterministic event serialization
* rehydration correctness
* optimistic concurrency enforcement
* snapshot policy if needed
* schema alignment
* audit persistence linkage
* replay determinism checks
* recovery integrity checks

**2.8.16 Messaging and Kafka layer**

* WhyceID command topics
* WhyceID event topics
* retry topics
* deadletter topics
* topic naming compliance
* event contract registration
* canonical header contract
* outbox integration
* retry behavior
* DLQ behavior

**2.8.17 Projection and read-model layer**

* identity profile read model
* auth/readiness read model
* authorization read model
* trust read model
* verification read model
* consent read model
* session read model
* device read model
* graph read model
* service identity read model
* audit read model
* replay/catch-up validation

**2.8.18 Cross-domain integration layer**

* structural-system linkage
* economic-system actor binding
* content-system ownership/access binding
* policy-system identity input binding
* runtime/system/service actor binding
* workflow actor-state linkage where currently needed inside Phase 2
* audit and evidence continuity across domains
* anti-drift checks on identity propagation
* no-cross-domain identity duplication rules
* full project actor continuity checks

Registration propagation topics (new — triggered at registration/activation):

* identity registration → structural-system placement event (actor positioned in cluster topology)
* identity registration → economic-system account/wallet initialization event (actor gets economic standing)
* identity registration → content-system default-profile initialization (actor content ownership anchor)
* organization registration → structural-system org-node creation
* operator registration → structural-system operator-role assignment
* propagation ordering: identity MUST be fully activated before cross-domain events fire
* propagation failure containment: registration does not roll back if downstream domain propagation fails (eventual consistency with explicit reconciliation)
* propagation audit: every cross-domain init event is evidence-linked to the originating registration event

**2.8.19 Security and hardening layer**

* secret handling rules
* credential protection rules
* token hardening
* MFA enforcement controls
* anti-enumeration protections
* throttling and lockout controls
* privileged access hardening
* session anomaly protections
* device anomaly protections
* service credential rotation hardening

**2.8.20 Observability layer**

* authentication metrics
* authorization metrics
* trust metrics
* verification metrics
* consent metrics
* session/device metrics
* service identity metrics
* audit completeness metrics
* denial/failure signals
* drift/anomaly signals

**2.8.21 Testing and certification layer**

* aggregate tests for each engine
* invariant tests for each engine
* replay determinism tests
* middleware integration tests
* policy integration tests
* messaging tests
* projection correctness tests
* API end-to-end tests
* resilience tests
* regression certification pack

**2.8.22 Resilience validation layer**

* duplicate registration resistance
* duplicate command/idempotency resistance
* restart and replay recovery
* session recovery correctness
* Kafka outage recovery
* Postgres outage recovery
* projection catch-up recovery
* partial failure containment
* service credential failure recovery
* cross-domain identity consistency after failure

**2.8.23 Documentation and anti-drift layer**

* WhyceID canonical README set
* engine-by-engine documentation
* actor taxonomy documentation
* role/permission documentation
* identity graph documentation
* command/event catalog
* policy catalog
* API catalog
* guard updates
* audit updates
* completion evidence pack

**2.8.24 Phase 2.8 completion criteria**

* all WhyceID engines implemented canonically
* registration and onboarding workflow layer complete (2.8.1a)
* authentication including credential reset and account recovery complete (2.8.2)
* runtime integration complete
* policy integration complete
* persistence/messaging/projections complete
* platform API complete
* cross-domain integration complete including registration propagation
* audit and evidence complete
* regression pack passing (including full registration-to-onboarding E2E)
* resilience validation passing
* completion evidence produced

---

**2.8.25 Platform bootstrapping and initialization**
Purpose:

* govern the identity state of the platform itself at first startup
* ensure the first operator identity is created safely and is not a security gap

Implementation topics:

* super-admin / first-operator initialization model
* platform seed identity: deterministic, policy-bound, no anonymous bootstrap
* first-operator credential bootstrap (how is the first operator set up without an existing operator to authorize it)
* platform initialization event: `PlatformIdentityBootstrappedEvent`
* bootstrapping policy: single-use, time-bounded, audited
* post-bootstrap state: platform-level service identities for background workers, replay runners, and scheduled agents
* bootstrapping audit trail (WhyceChain anchored)
* no-re-bootstrap invariant (once bootstrapped, cannot be re-run without explicit operator-authorized reset)

Completion proof:

* platform can start from zero and bootstrap first identity correctly
* bootstrap is idempotent (repeat attempt is rejected)
* all background service identities are declared and provisioned at startup
* no anonymous system-level operations remain after bootstrap

The best execution order is:

**Batch A — identity core**

* foundation
* Identity Engine
* Authentication Engine
* Session Engine

**Batch B — access control**

* Authorization Engine
* Device Engine
* ServiceIdentity Engine

**Batch C — governance**

* TrustScore Engine
* Verification Engine
* Consent Engine
* IdentityGraph Engine

**Batch D — system wiring**

* Audit Engine
* policy integration
* runtime integration
* platform API integration
* persistence/messaging/projections

**Batch E — project proof**

* cross-domain integration
* security and hardening
* observability
* testing and certification
* resilience validation
* documentation and anti-drift

The strongest way to track it is with this condensed implementation sequence:

1. Identity (2.8.1) + Platform Bootstrap (2.8.25)
2. Registration and Onboarding Workflow (2.8.1a) ← new, fills the registration/onboarding gap
3. Authentication including credential reset + account recovery (2.8.2)
4. Session (2.8.7)
5. Authorization (2.8.3)
6. Device (2.8.8)
7. ServiceIdentity (2.8.10)
8. TrustScore (2.8.4)
9. Verification (2.8.5)
10. Consent (2.8.6)
11. IdentityGraph (2.8.9)
12. Audit (2.8.11)
13. Policy + Runtime + API + Persistence + Messaging + Projections (2.8.12–2.8.17)
14. Cross-domain integration including registration propagation (2.8.18)
15. Security, Observability, Resilience, Testing (2.8.19–2.8.22)
16. Documentation + Certification (2.8.23–2.8.24)

A good canonical one-line statement for this phase is:

**Phase 2.8 implements the full WhyceID engine set end to end across the project, then wires it through policy, runtime, persistence, messaging, projections, API, and cross-domain integration.**

The next natural step is **Phase 2.9 for WhyceChain** so the full T0U trio is sequenced cleanly inside Phase 2.
