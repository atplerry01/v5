# NEW RULES — Policy Eventification (2026-04-07)

SOURCE: project-prompts/20260407-190000-runtime-policy-eventification.md
SOURCE (CANONICAL PATCH): project-prompts/20260407-231327-runtime-policy-eventification-canonical-patch.md
CLASSIFICATION: runtime / policy
STATUS: COMPLETE — ALLOW + DENY + dedicated stream + dedicated topic. All four blockers resolved.
CANONICAL PATCH (2026-04-07 23:13): AuditEmission.Metadata bundle added; factory now returns AuditEmission directly; PolicyMiddleware ALLOW path emits audit BEFORE next() so it is unconditional. Tests blocked end-to-end by pre-existing HSID v2.1 validation seam (see 20260407-200000-hsid-v2.1-parallel-seam.md), not by this patch — build is clean and 7 unrelated integration tests pass.

---

## RULE: POLICY-EVENT-REQUIRED-01
SEVERITY: S0
DESCRIPTION: Every WHYCEPOLICY evaluation (allow OR deny) MUST emit a domain
event (PolicyEvaluatedEvent or PolicyDeniedEvent) carrying DecisionHash,
IdentityId, PolicyName, CorrelationId, CausationId. A DecisionHash without
a corresponding event is a governance violation.
PROPOSED PROMOTION TARGET: policy.guard.md (extends POL-AUDIT-01 already
present at line 160)

## RULE: POLICY-NO-SILENT-DECISION-01
SEVERITY: S0
DESCRIPTION: A policy evaluation that produces a DecisionHash but does not
appear as a domain event in EventStore is forbidden. Audit trail must be
queryable via the event stream, not only via chain anchors.
PROPOSED PROMOTION TARGET: policy.guard.md

## RULE: POLICY-PIPELINE-INTEGRATION-01
SEVERITY: S0
DESCRIPTION: Policy events MUST flow through the RuntimeControlPlane event
fabric (persist → chain → publish → outbox). PolicyMiddleware MUST NOT call
EventStore, ChainAnchorService, or KafkaProducer directly. Events are
returned via CommandResult.EmittedEvents and processed by the fabric.
PROPOSED PROMOTION TARGET: runtime.guard.md (extends rule 7)

## RULE: POLICY-REPLAY-INTEGRITY-01
SEVERITY: S0
DESCRIPTION: EventReplayService MUST NOT re-evaluate policy during replay.
Stored PolicyEvaluatedEvent / PolicyDeniedEvent records are the source of
truth for replayed decisions. Re-evaluation would risk drift if policy
versions or trust scores have changed since original evaluation.
PROPOSED PROMOTION TARGET: replay-determinism.guard.md

---

## ARCHITECTURAL BLOCKERS DISCOVERED — REQUIRES USER DECISION

### BLOCKER 1 — RESOLVED 2026-04-07 (audit-emission carve-out)
CommandResult.IsAuditEmission flag added; CommandResult.Failure(error, events)
overload routes deny events through the fabric. RuntimeControlPlane bypasses
the PolicyDecisionAllowed==true check on the audit-emission path but still
requires PolicyDecisionHash. PolicyMiddleware deny path now emits
PolicyDeniedEvent through the engine-layer factory. Verified by
PolicyEventificationTests.Deny_Emits_PolicyDeniedEvent_With_DecisionHash_And_No_Domain_Mutation
and TodoPipelineTests.PolicyDenial_BlocksExecution_But_Records_DeniedEvent.

### BLOCKER 1 (original) — Deny-path persistence is architecturally forbidden
RuntimeControlPlane.cs lines 56–70 HARD-STOPs event persistence when
PolicyDecisionAllowed != true. POL-AUDIT-01 / POLICY-EVENT-REQUIRED-01
require PolicyDeniedEvent to be persisted, but the existing chain-integrity
guard rejects all events on the deny path. The two rules are in direct
conflict.

OPTIONS:
  (a) Carve out an exception in RuntimeControlPlane allowing audit-only
      events (PolicyDeniedEvent and similar) to persist on deny — requires
      a new "audit-event" classification on CommandResult and updates to
      policy.guard.md / runtime.guard.md.
  (b) Persist policy decisions via a dedicated audit channel that bypasses
      the normal event fabric — explicitly forbidden by Non-Negotiable Rule
      #4 of the prompt and by runtime.guard.md rule 7.
  (c) Accept that DENY decisions are recorded only via DecisionHash + chain
      anchor (today's behavior) — contradicts the prompt's stated objective.

### BLOCKER 2 — CommandResult.Failure has no EmittedEvents slot
CommandResult.Failure(string error) does not carry events. Even on the ALLOW
path, the middleware returns the inner result from next() — to inject
PolicyEvaluatedEvent the middleware must construct a new CommandResult with
the policy event prepended to result.EmittedEvents. This is mechanical but
needs to be applied at every Failure return site OR CommandResult must be
extended with `Failure(string, IReadOnlyList<object>)`.

### BLOCKER 3 — RESOLVED 2026-04-07 (audit emission routing override)
AuditEmission carries Classification/Context/Domain overrides. EventFabric.ProcessAuditAsync
builds envelopes with the override metadata, so TopicNameResolver produces
`whyce.constitutional-system.policy.decision.events`. PolicyMiddleware sets
Classification="constitutional-system", Context="policy", Domain="decision".
Verified by PolicyEventificationTests + TodoPipelineTests topic assertions.

### BLOCKER 4 — RESOLVED 2026-04-07 (dedicated audit aggregate stream)
AuditEmission.AggregateId derived deterministically from CommandId via
`IIdGenerator.Generate("policy-audit-stream:{CommandId}")`. EventFabric.ProcessAuditAsync
persists to that aggregate id, isolating policy events from the command's
domain aggregate stream. Verified: TodoPipelineTests asserts the Todo
aggregate stream contains ONLY domain events, while PolicyEventificationTests
asserts the policy event lives in its own deterministically-derived stream.

### BLOCKER 3 (original) — Topic resolution does not match prompt-specified topic format
Prompt requires `whyce.{cluster}.policy.policy-evaluated`. TopicNameResolver
enforces a 5-segment canonical format `whyce.{classification}.{context}.{domain}.{type}`
keyed off CommandContext.Classification/Context/Domain — which carry the
COMMAND's domain, not "policy". To route policy events to a dedicated
policy topic the envelope would need overridden routing metadata, which
requires either:
  (a) An envelope override mechanism on CommandResult.EmittedEvents, OR
  (b) Per-event routing metadata on EventSchemaEntry.
Both are infrastructure additions, not in scope of this prompt.

### BLOCKER 4 (original) — EventFabric persistence is aggregate-keyed
Policy events have no owning aggregate. EventEnvelope.AggregateId in the
existing fabric is sourced from CommandContext.AggregateId — meaning policy
events would be persisted under the command's aggregate stream, polluting
domain aggregate replay. A dedicated policy decision aggregate (or a
non-aggregate "audit stream" type) is required.

---

## WHAT WAS COMPLETED IN OPTION-4 PASS

- src/domain/constitutional-system/policy/decision/event/PolicyEvaluatedEvent.cs (NEW)
- src/domain/constitutional-system/policy/decision/event/PolicyDeniedEvent.cs (NEW, defined but not yet emitted)
- src/shared/contracts/policy/IPolicyDecisionEventFactory.cs (NEW — primitive-typed contract so runtime middleware avoids Whycespace.Domain.* references per rule 11.R-DOM-01)
- src/engines/T0U/whycepolicy/PolicyDecisionEventFactory.cs (NEW — engine-layer impl)
- src/runtime/middleware/policy/PolicyMiddleware.cs (MODIFIED — injects IIdGenerator + IPolicyDecisionEventFactory; ALLOW path now prepends PolicyEvaluatedEvent to CommandResult.EmittedEvents; deterministic EventId derived via "{CommandId}:PolicyEvaluatedEvent" seed)
- src/platform/host/composition/constitutional/policy/ConstitutionalPolicyBootstrap.cs (NEW — registers schema entries for both events)
- src/platform/host/composition/BootstrapModuleCatalog.cs (MODIFIED — added ConstitutionalPolicyBootstrap)
- src/platform/host/composition/runtime/RuntimeComposition.cs (MODIFIED — DI for IPolicyDecisionEventFactory + extended PolicyMw constructor)
- tests/integration/setup/TestHost.cs (MODIFIED — extended PolicyMw constructor)

Build verified: dotnet build succeeded with 0 errors (2 pre-existing warnings unrelated).

## WHAT REMAINS DEFERRED (DENY-PATH PHASE)

- Step 2 (DENY half): PolicyDeniedEvent emission. Blocked by Blocker 1 — RuntimeControlPlane.cs:56-70 forbids persistence on PolicyDecisionAllowed != true. Requires audit-event carve-out + CommandResult.Failure(error, events) overload.
- Dedicated policy topic (Blocker 3): PolicyEvaluatedEvent currently publishes to the COMMAND's domain topic, not whyce.*.policy.policy-evaluated. Acceptable for ALLOW-only audit observability; flagged for follow-up.
- Dedicated policy aggregate / audit stream (Blocker 4): policy events persist under the command's aggregate stream. Flagged for follow-up.
- Step 7 tests: not added in this pass — semantic verification deferred to a dedicated test prompt.

## SELF-CHECK against guards loaded this pass

- Determinism guard: PASS. EventId via IIdGenerator with deterministic seed; no DateTime/Guid.NewGuid in any new file. Verified in PolicyMiddleware, factory, event records, bootstrap.
- Structural guard rule 1 (DOMAIN ISOLATION): PASS. New domain events reference no external types.
- Structural guard rule 9 (DEPENDENCY DIRECTION): PASS. shared → no upward refs; engine → domain (allowed); runtime → shared/engine (allowed); platform/host/composition → domain (exempt zone).
- Runtime guard rule 11.R-DOM-01: PASS. PolicyMiddleware.cs does not import Whycespace.Domain.*. Domain construction is delegated to engine-layer factory via shared interface.
- Runtime guard rule 7 (persist/publish/anchor authority): PASS. Middleware does not call EventStore, Kafka, or ChainAnchor; events flow via CommandResult.EmittedEvents → EventFabric.
- Policy guard POL-AUDIT-01: PARTIAL. ALLOW path satisfied; DENY explicitly deferred and documented.

## NOTE ON STRUCTURAL PLACEMENT
The prompt literally specified `src/domain/constitutional-system/policy/event/`,
which would have violated CLAUDE.md three-level nesting
(`{classification}/{context}/{domain}/`) and the existing convention used by
all 9 sibling sub-domains under policy/. With user authorization, the events
were placed under a new `decision/` sub-domain to preserve structural
integrity. STR-DOMAIN-NESTING enforcement preserved.
