You are performing a STRICT REMEDIATION AND COMPLETION PASS for a Whycespace domain-group implementation that has already been audited.

Your job is to FIX the implementation so it becomes fully end-to-end complete and canonically correct.

This is not a review.
This is not a discussion.
This is not a suggestion list.

This is a targeted implementation and repair operation.

If the audit found missing or incorrect implementation, you must:
- identify every blocking defect
- patch the implementation completely
- wire missing layers end to end
- preserve canonical architecture
- avoid drift
- produce a clean change report

--------------------------------------------------
0. INPUTS: use @/pipeline/execution_context.md
--------------------------------------------------

Classification: <classification>
Context: <context>
Domain Group: <domain-group>
Domains:
- <domain-1>
- <domain-2>
- <domain-3>

Audit Result:
<paste the full FAIL / partial audit here>

Implementation Goal:
Bring this batch to full canonical end-to-end completion:
API → Runtime → Engine → Domain → Event Store → Chain → Outbox → Kafka → Projection → API Response

--------------------------------------------------
1. EXECUTION MODE
--------------------------------------------------

Work in FIX mode, not analysis mode.

Do not restate the audit at length.
Do not produce generic advice.
Do not stop at identifying problems.

You must produce the concrete implementation changes required to make the batch PASS.

Where possible, modify existing files instead of introducing unnecessary new ones.
Where missing files are required, create them canonically.
Do not create duplicate structures.
Do not invent alternate architecture.

--------------------------------------------------
2. MANDATORY CANONICAL RULES
--------------------------------------------------

You must obey all of the following:

A. Structure
- preserve canonical hierarchy:
  CLASSIFICATION → CONTEXT → DOMAIN GROUP → DOMAIN
- do not flatten domain groups
- do not move domains outside canonical path

B. Domain purity
- no infrastructure concerns in domain layer
- no DbContext
- no EF Core
- no Dapper
- no HttpClient
- no persistence logic in domain model

C. Determinism
- no Guid.NewGuid()
- no DateTime.UtcNow / DateTime.Now / DateTimeOffset.UtcNow
- no random behavior
- no non-deterministic IDs
- respect approved deterministic ID and time abstractions

D. Runtime doctrine
- preserve canonical execution pipeline
- do not bypass dispatcher/runtime/middleware
- do not call domain directly from controller
- do not bypass outbox/event fabric
- do not bypass policy gates where required

E. Topic doctrine
Use canonical topic naming only:
whyce.<classification>.<context>.<domain>.<channel>

Canonical channels:
- commands
- events
- retry
- deadletter

F. Event fabric order
Must remain:
persist → chain anchor → outbox

G. Workflow doctrine
Add workflow only if justified.
Do not introduce T1M orchestration for actions that should stay T2E-only.

--------------------------------------------------
3. REMEDIATION TASKS
--------------------------------------------------

Using the audit findings, fix all blocking defects across these layers:

A. DOMAIN MODEL (E1)
For each affected domain:
- fix aggregate structure
- fix invalid state transitions
- add missing events
- add missing invariants
- add missing specifications
- add missing domain errors
- correct value object usage
- remove non-canonical patterns
- fix README if needed

B. COMMAND LAYER (E2)
- add missing commands
- fix naming or payload issues
- remove business logic leakage from DTOs

C. QUERY LAYER (E3)
- add missing queries/read contracts
- restore CQRS separation
- ensure read paths are projection-backed where required

D. ENGINE HANDLERS (E4)
- add missing handlers
- fix incorrect routing
- fix aggregate invocation
- fix deterministic execution issues
- fix idempotency concerns where applicable

E. POLICY INTEGRATION (E5)
- add missing policy checks
- define missing policy action names
- fix broken authorization boundaries

F. EVENT FABRIC (E6)
- fix persistence/publish flow
- ensure outbox is used
- ensure chain anchoring is present
- correct topic names
- ensure required headers exist:
  - event-id
  - aggregate-id
  - event-type
  - correlation-id

G. PROJECTIONS (E7)
- add missing projection handlers
- add/update read models
- fix Kafka → projection subscriptions
- ensure projected data is queryable

H. API LAYER (E8)
- add missing controllers/endpoints
- fix request/response contracts
- ensure controller → dispatcher wiring is correct
- ensure response shape is operationally valid

I. WORKFLOW (E9, only if justified)
- add or fix workflow only if the domain truly needs orchestration
- remove workflow misuse if unnecessary

J. OBSERVABILITY / SECURITY / E2E (E10–E12)
- add missing trace/log/metric hooks where applicable
- ensure identity/policy/security boundaries are enforced
- add or fix E2E verification path

--------------------------------------------------
4. REQUIRED OUTPUT FORMAT
--------------------------------------------------

Return your result in this exact structure:

1. Remediation Summary
2. Blocking Defects Fixed
3. Files Created
4. Files Modified
5. Stage-by-Stage Fixes
6. End-to-End Wiring Confirmation
7. Validation Commands
8. Remaining Gaps
9. Final Verdict

--------------------------------------------------
5. STAGE-BY-STAGE FIX FORMAT
--------------------------------------------------

For each affected stage, use this structure:

STAGE <E#> — <name>

Status:
- FIXED / NOT NEEDED / DEFERRED

Problems found:
- <specific defect 1>
- <specific defect 2>

Changes made:
- <exact change 1>
- <exact change 2>

Files affected:
- <path>
- <path>

Why this now passes:
- <brief explanation>

--------------------------------------------------
6. FILE OUTPUT RULES
--------------------------------------------------

When producing fixes:
- show exact folder trees where structure changed
- show production-ready code only
- do not use pseudo-code unless explicitly requested
- do not leave placeholder stubs pretending to be complete
- do not create vague helpers or misc files
- keep names explicit and domain-driven

If the audit references incorrect files, preserve good files and patch only what is necessary.
Prefer minimal-correct refactoring over broad unnecessary rewrites.

--------------------------------------------------
7. END-TO-END VALIDATION REQUIREMENTS
--------------------------------------------------

You must provide executable validation steps for the repaired implementation.

Include:

A. Example API call
Provide curl command(s) that exercise the repaired batch.

B. Expected API response
Show the expected success response shape.

C. Persistence verification
Provide DB verification query or equivalent check for:
- aggregate/event persistence
- outbox record if applicable
- projection/read model update

D. Kafka verification
State:
- expected topic
- expected event type
- expected key headers

E. Read-model verification
Show how to confirm the projection was updated.

F. Runtime path confirmation
Explicitly confirm the intended path:
API → Dispatcher → Runtime Control Plane → Handler → Aggregate → Event Store → Chain → Outbox → Kafka → Projection → Response

If any of these are still not possible after your changes, clearly state what remains blocking.

--------------------------------------------------
8. SELF-CHECK BEFORE FINALIZING
--------------------------------------------------

Before final output, verify:

- all blocking audit defects addressed
- no domain purity violations
- no deterministic violations
- no wrong topic names
- no missing handlers for claimed API actions
- no controller bypass of dispatcher/runtime
- no fake “complete” projection path
- no unjustified workflow added
- response includes validation commands
- response includes final production-readiness verdict

--------------------------------------------------
9. FINAL VERDICT RULE
--------------------------------------------------

You must end with exactly one of the following:

PASS:
“This domain group has been remediated to canonical end-to-end completion and is ready for verification.”

PARTIAL:
“This domain group has been substantially remediated, but blocking gaps remain before end-to-end PASS.”

FAIL:
“This domain group remains blocked. Critical defects still prevent canonical end-to-end execution.”