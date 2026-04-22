

You are implementing domain model work for the Whycespace project.

Work strictly within the locked canonical architecture and repository doctrine already established for the project.

Your task is to implement ONE BATCH at a time.

BATCH DEFINITION
A batch must cover:
- exactly one CLASSIFICATION
- exactly one CONTEXT inside that classification
- exactly one DOMAIN GROUP inside that context
- all DOMAINS inside that domain group

Canonical modelling hierarchy:
CLASSIFICATION → CONTEXT → DOMAIN GROUP → DOMAIN

Do not flatten this structure.
Do not skip the domain group layer.
Do not invent extra hierarchy unless explicitly specified.

--------------------------------------------------
1. INPUTS FOR THIS BATCH use @/pipeline/execution_context.md
--------------------------------------------------

CLASSIFICATION: <classification>
CONTEXT: <context>
DOMAIN GROUP: <domain-group>
DOMAINS:
- <domain-1>
- <domain-2>
- <domain-3>

Optional batch description:
<short business / system purpose of this domain group>

--------------------------------------------------
1. OBJECTIVE
--------------------------------------------------

Implement this domain-group batch in staged form using the project’s canonical E1 → EX implementation flow.

Each domain in the batch must be implemented consistently and to the correct maturity for the requested stage.

Unless explicitly stated otherwise:
- domain model must follow S4 standard minimum
- implementation must remain deterministic
- no external/infrastructure dependency is allowed inside the domain model
- all naming, paths, namespaces, and topic contracts must remain canonical
- all work must align with WhyceID, WHYCEPOLICY, WhyceChain, and economic/policy-governed execution doctrine where relevant

--------------------------------------------------
3. REQUIRED OUTPUT FORMAT
--------------------------------------------------

Return your work in this exact structure:

1. Batch Summary
2. Scope Confirmation
3. Stage-by-Stage Implementation Plan
4. Files to Create / Modify
5. Domain Rules / Invariants
6. Integration Notes
7. Validation Checklist
8. Change Report
9. Risks / Gaps / Follow-up Items

Where code or folder structures are requested, provide production-ready outputs, not placeholders.

--------------------------------------------------
4. CANONICAL IMPLEMENTATION RULES
--------------------------------------------------

You must obey all of the following rules:

A. Modelling Structure
- Use the exact hierarchy:
  CLASSIFICATION → CONTEXT → DOMAIN GROUP → DOMAIN
- Every feature must live under the correct classification and context
- Do not place a domain in the wrong context
- Do not merge unrelated domains simply for convenience

B. Domain Purity
- Domain layer must contain no infrastructure dependencies
- No DbContext
- No Entity Framework
- No Dapper
- No HttpClient
- No external service calls
- No Microsoft infrastructure concerns inside domain logic
- No persistence logic in domain models

C. Determinism
- No Guid.NewGuid()
- No DateTime.UtcNow
- No DateTime.Now
- No DateTimeOffset.UtcNow
- No random number generation
- No non-deterministic runtime behavior
- Time must come from approved clock abstractions outside the domain where needed
- IDs must follow approved deterministic generation rules

D. Domain Standard
Unless the batch explicitly requires otherwise, each domain should be structured to S4 standard using the appropriate subset of:
- aggregate/
- entity/
- error/
- event/
- service/
- specification/
- value-object/
- README.md

Only include folders that are justified by the domain.
Do not create meaningless placeholder files.

E. Invariants and Specifications
- Every aggregate must enforce business invariants
- Specifications must contain reusable business rules
- Errors must be explicit and domain-specific
- Events must reflect meaningful state transitions
- State transitions must be valid, auditable, and deterministic

F. Runtime / System Alignment
The implementation must align with the canonical execution model:
API → Runtime → Engine → Domain → Event Store → Chain Anchor → Outbox → Kafka → Projection → Response

G. Policy / Identity / Chain
Where relevant, ensure the design supports:
- actor identification
- policy evaluation
- authorization / entitlement checks
- event emission
- chain anchoring
- auditability
- anti-bot / trust-aware enforcement where applicable

H. Workflows
Only introduce T1M workflow support where justified.
Do not add workflow orchestration for simple bounded domain actions that can remain T2E-only.

Valid workflow examples:
- lifecycle orchestration
- long-running coordination
- cross-domain compensation
- payout / settlement progression
- approval flows
- stateful multi-step execution

Invalid workflow usage:
- simple CRUD-like aggregate actions
- single-aggregate direct command handling without orchestration need

--------------------------------------------------
5. STAGE EXECUTION MODEL
--------------------------------------------------

Implement the batch in stages.

For each stage:
- explain what is included
- list affected files
- provide concrete implementation output
- state what is intentionally deferred

Use the following stage model:

STAGE E1 — DOMAIN IMPLEMENTATION
For every domain in this batch:
- define aggregate root where applicable
- define events
- define value objects
- define errors
- define specifications
- define domain services only where truly required
- define state transitions and invariants
- add or update README.md
- ensure no infrastructure leakage

Deliverables:
- production-ready folder tree
- core domain classes
- invariants/specifications/errors/events
- README coverage

STAGE E2 — COMMAND LAYER
For every actionable domain operation:
- define commands
- define command contracts and intent shape
- map commands to domain behavior
- preserve deterministic field requirements

Deliverables:
- create/update/archive/activate/etc commands
- command naming consistent with domain language

STAGE E3 — QUERY LAYER
For every required read use case:
- define queries
- define query DTO/read contracts
- separate read concerns from write concerns
- respect CQRS boundaries

Deliverables:
- get/list/detail/progress/history queries as applicable

STAGE E4 — T2E ENGINE HANDLERS
- implement execution handlers for commands
- map command intent to aggregate operations
- maintain deterministic execution
- preserve middleware and control-plane assumptions

Deliverables:
- command handlers
- execution mapping notes
- idempotency/replay considerations where relevant

STAGE E5 — POLICY INTEGRATION
- identify required policy gates
- define the policy decision points
- specify permissions / entitlements / restrictions
- align to WHYCEPOLICY doctrine

Deliverables:
- policy action list
- policy rule names
- enforcement notes

STAGE E6 — EVENT FABRIC INTEGRATION
- map each domain event to canonical topic naming
- define commands/events/retry/deadletter channel expectations
- ensure naming aligns with the project’s topic doctrine

Deliverables:
- topic map per domain
- event publication contracts
- routing notes

STAGE E7 — PROJECTIONS
- identify read models needed
- define projection responsibilities
- separate source-of-truth from read optimization

Deliverables:
- projection/read-model plan
- table/cache/read-model naming
- event-to-projection mapping

STAGE E8 — API LAYER
- define the platform API surface needed for this batch
- map commands and queries to endpoints
- keep API aligned to canonical routing conventions

Deliverables:
- endpoint list
- request/response contract notes
- controller/application mapping notes

STAGE E9 — WORKFLOW LAYER
Only if justified:
- define lifecycle or orchestration workflows
- specify triggers, state transitions, retries, and completion criteria

Deliverables:
- workflow definitions
- step orchestration notes
- reasons workflow is justified

STAGE E10 — OBSERVABILITY
- define traces, logs, metrics, and audit signals
- identify business and system signals worth measuring

Deliverables:
- metrics list
- tracing path
- structured logging/audit notes

STAGE E11 — SECURITY & ENFORCEMENT
- identify identity requirements
- define access control boundaries
- note trust-score / anti-bot / restricted-operation implications where applicable

Deliverables:
- security enforcement notes
- access assumptions
- restricted action matrix if needed

STAGE E12 — E2E VALIDATION
- define end-to-end validation path
- include API → runtime → engine → domain → event store → projection checks
- specify what to verify in Kafka, projections, event persistence, and response

Deliverables:
- E2E validation checklist
- curl/postman examples if requested
- verification points in persistence/messaging/projections

STAGE E13–E16 — PHASE 2+ ENHANCEMENTS
Do not implement unless explicitly requested.
Only outline:
- cross-domain orchestration
- advanced economic routing
- multi-cluster distribution
- advanced governance or policy depth

STAGE E17–EX — ADVANCED / DEFERRED
Do not implement unless explicitly requested.
Only outline:
- intelligence features
- predictive systems
- autonomous assistance
- recommendation/personalization/AI-driven capabilities

--------------------------------------------------
6. PER-DOMAIN EXECUTION TEMPLATE
--------------------------------------------------

For each domain in the batch, use this structure:

Domain: <domain-name>

1. Purpose
- what this domain governs
- why it exists inside this domain group

2. Aggregate Model
- aggregate name
- state
- lifecycle/status model
- entities if any
- value objects

3. Events
- created
- updated
- transitioned
- closed/completed/archived/etc as applicable

4. Invariants
- list the non-negotiable business rules

5. Specifications
- reusable business rule checks

6. Errors
- explicit domain failure cases

7. Commands
- write operations for this domain

8. Queries
- read operations for this domain

9. Projection Needs
- read model(s) required

10. Policy / Identity / Access Needs
- who can act
- what must be checked
- entitlement / role / trust implications

11. API Surface
- expected endpoints

12. E2E Validation Notes
- what success looks like for this domain

--------------------------------------------------
7. FILE AND FOLDER GENERATION RULES
--------------------------------------------------

When generating folder structures:
- produce exact tree output
- include only justified files
- preserve canonical naming
- keep names explicit and domain-driven
- do not use vague placeholder names like Helper, Utils, Misc, CommonStuff

When generating code:
- make it production-ready
- no pseudo-code unless explicitly requested
- no TODO placeholders unless marking intentionally deferred future work
- no fake implementations
- no stubs pretending to be complete

When updating existing work:
- preserve canonical files where already correct
- refactor only where needed
- explain each structural correction clearly

--------------------------------------------------
8. VALIDATION / SELF-AUDIT BEFORE FINAL OUTPUT
--------------------------------------------------

Before finalizing, verify all of the following:

- correct classification
- correct context
- correct domain group
- all requested domains included
- no domain placed outside canonical path
- no infrastructure leakage in domain layer
- no non-deterministic APIs used
- invariants are explicit
- errors are explicit
- events reflect actual business transitions
- topic naming is canonical
- workflow added only where justified
- output is complete and production-ready

If anything is missing, state it clearly under:
“Gaps / Deferred Items”

--------------------------------------------------
9. FINAL RESPONSE INSTRUCTION
--------------------------------------------------

Do not give a shallow summary.

Produce a serious implementation-grade response suitable for direct project execution.

Be strict, canonical, deterministic, and architecture-aligned.

If the batch is large, still complete the whole batch, but organize the answer domain-by-domain and stage-by-stage.

If there is a structural conflict, follow project canon and explicitly call out the conflict before resolving it.

Start now with:
- Scope Confirmation
- Batch Summary
- Stage E1