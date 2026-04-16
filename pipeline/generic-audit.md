You are performing a FULL END-TO-END IMPLEMENTATION AUDIT for a Whycespace domain model batch.

##
Classification: economic-system
Context: reconciliation
Domain Group: control
Domains:
- discrepancy
- process
##

This is NOT a review.
This is NOT a summary.

This is a STRICT VERIFICATION that the implementation is:
- architecturally correct
- deterministically safe
- fully wired
- operationally complete
- and provably executable end-to-end

If ANY part is missing, incorrect, or non-canonical → you MUST FAIL the audit.

--------------------------------------------------
1. AUDIT SCOPE use @/pipeline/execution_context.md
--------------------------------------------------

Classification: <classification>
Context: <context>
Domain Group: <domain-group>
Domains:
- <domain-1>
- <domain-2>

Audit Target:
Verify that this domain group is implemented END-TO-END:
API → Runtime → Engine → Domain → Event Store → Chain → Outbox → Kafka → Projection → API Response

--------------------------------------------------
1. VERDICT FORMAT (MANDATORY)
--------------------------------------------------

You MUST start with:

FINAL VERDICT: PASS / FAIL

If FAIL:
- clearly state blocking defects
- list exact files/components missing or incorrect
- do NOT soften language

--------------------------------------------------
2. STAGE-BY-STAGE AUDIT
--------------------------------------------------

You MUST validate each stage below.

If a stage is partially implemented → mark FAIL.

-------------------------------------
A. DOMAIN MODEL (E1)
-------------------------------------

Verify for EACH domain:

- Aggregate exists and is correctly structured
- State model is complete and valid
- Events exist and represent real state transitions
- Invariants are enforced inside aggregate
- Specifications are reusable and correct
- Errors are explicit and domain-specific
- Value Objects are used where appropriate

STRICT CHECKS:
- ❌ No DbContext / EF / Dapper / infrastructure usage
- ❌ No Guid.NewGuid()
- ❌ No DateTime.UtcNow / Now
- ❌ No random or non-deterministic logic

RESULT:
PASS / FAIL

-------------------------------------
B. COMMAND LAYER (E2)
-------------------------------------

Verify:

- Commands exist for all domain actions
- Command structure is consistent and deterministic
- No business logic leakage into command DTOs

RESULT:
PASS / FAIL

-------------------------------------
C. QUERY LAYER (E3)
-------------------------------------

Verify:

- Queries exist for required read paths
- CQRS separation is respected
- No domain mutation inside queries

RESULT:
PASS / FAIL

-------------------------------------
D. ENGINE HANDLERS (E4)
-------------------------------------

Verify:

- Command handlers exist
- Handlers correctly invoke aggregates
- Routing uses canonical DomainRoute
- Idempotency is respected

RESULT:
PASS / FAIL

-------------------------------------
E. POLICY INTEGRATION (E5)
-------------------------------------

Verify:

- Policy checks exist before execution where required
- Policy actions are correctly named
- Authorization boundaries are respected

RESULT:
PASS / FAIL

-------------------------------------
F. EVENT FABRIC (E6)
-------------------------------------

Verify:

- Events are persisted before publish
- Chain anchoring exists
- Outbox pattern is used
- Kafka topics follow canonical naming:

  whyce.<classification>.<context>.<domain>.events

- Headers include:
  event-id
  aggregate-id
  event-type
  correlation-id

RESULT:
PASS / FAIL

-------------------------------------
G. PROJECTIONS (E7)
-------------------------------------

Verify:

- Read models exist
- Projection handlers exist
- Kafka → Projection flow is wired
- Data is queryable after events

RESULT:
PASS / FAIL

-------------------------------------
H. API LAYER (E8)
-------------------------------------

Verify:

- API endpoints exist
- Endpoints map correctly to commands/queries
- Request/response contracts are valid
- Controllers use dispatcher → NOT direct domain calls

RESULT:
PASS / FAIL

-------------------------------------
I. WORKFLOW (E9 — IF APPLICABLE)
-------------------------------------

Verify ONLY if workflow exists:

- Workflow is justified (not unnecessary)
- Steps are correctly orchestrated
- No duplication of domain logic

RESULT:
PASS / FAIL / N/A

-------------------------------------
J. OBSERVABILITY (E10)
-------------------------------------

Verify:

- Logging exists
- Tracing covers full execution path
- Metrics defined where relevant

RESULT:
PASS / FAIL

-------------------------------------
K. SECURITY & ENFORCEMENT (E11)
-------------------------------------

Verify:

- Identity is enforced
- Policy gating is active
- Restricted actions are controlled

RESULT:
PASS / FAIL

-------------------------------------
L. END-TO-END EXECUTION (E12)
-------------------------------------

You MUST simulate or verify this exact flow:

1. Send API request (curl or Postman)
2. Confirm:
   - Runtime pipeline executes (8 middleware stages)
   - Command reaches handler
   - Aggregate executes and emits event
   - Event persisted to Postgres
   - Event chain-anchored
   - Outbox entry created
   - Kafka receives event
   - Projection updates read model
   - API returns valid response

If ANY step is missing → FAIL

RESULT:
PASS / FAIL

--------------------------------------------------
3. REQUIRED EVIDENCE
--------------------------------------------------

You MUST provide:

- Example API request (curl)
- Expected API response
- Kafka topic used
- Projection table/read model name
- Example DB verification query
- Event sample (payload + headers)

--------------------------------------------------
4. CRITICAL DEFECTS
--------------------------------------------------

List ONLY blocking issues:

- missing handlers
- missing projections
- wrong topic naming
- non-deterministic logic
- policy not enforced
- API not wired
- event not emitted
- no end-to-end flow

--------------------------------------------------
5. CHANGE REPORT
--------------------------------------------------

List:

- Files missing
- Files incorrect
- Files needing refactor
- Exact fix actions required

--------------------------------------------------
6. FINAL STATEMENT
--------------------------------------------------

One of:

PASS:
“This domain group is fully implemented end-to-end and meets Whycespace canonical execution standards.”

FAIL:
“This domain group is NOT production-ready. Blocking defects prevent end-to-end execution.”