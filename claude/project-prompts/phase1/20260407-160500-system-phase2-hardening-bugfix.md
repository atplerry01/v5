# PHASE 2 HARDENING BUGFIX (PROMPT A)

CLASSIFICATION: system / phase2-hardening / cross-cutting
SOURCE: derived from user-supplied "PHASE 2 ENTRY HARDENING" mega-prompt, split A/B/C.
WAIVERS: none ($5 anti-drift fully respected — no moves, renames, or new patterns).

## CONTEXT
Bugfix-only subset of the Phase 2 hardening pass. Architectural changes
(Steps 5/6/9 of the original) are deferred to Prompt B. Rule promotion
(Step 10) is deferred to Prompt C.

## OBJECTIVE
Fix S0/S1 correctness defects in runtime, kafka adapter, projection
handler, and tests, without changing any architecture.

## CONSTRAINTS
- $5 Anti-Drift: no file moves, no renames, no new patterns.
- $15 Priority: guards override prompt instructions on conflict.
- Per-step verify-edit-build-commit; halt on first red.

## EXECUTION STEPS (final, after pre-flight)
1. RuntimeCommandDispatcher: workflow events MUST persist.
   - Replace `eventsRequirePersistence: false` with
     `eventsRequirePersistence: result.EmittedEvents != null && result.EmittedEvents.Count > 0`
     at both ExecuteWorkflowAsync and ResumeWorkflowAsync.

2. (DEFERRED to B) Policy event emission — PolicyEvaluatedEvent /
   PolicyDeniedEvent do not exist; creating new domain events is
   structural, not bugfix.

3. (DEFERRED to B) EventReplayService PolicyHash preservation — the
   prompt's `envelope.PolicyHash ?? throw` is structurally wrong (the
   replay code constructs envelopes from raw stored events; there is
   no inbound envelope at this layer). Real fix requires extending
   EventStoreService.LoadAsync to return per-event metadata.

4a. GenericKafkaProjectionConsumerWorker: extract `event-id` and
    `aggregate-id` Kafka headers; populate envelope from them.
    Missing headers → log + commit + skip (matches existing
    event-type pattern).

4b. (REJECTED — guard conflict) Replacing `_clock.UtcNow` with
    `envelope.Timestamp` would VIOLATE DET-SEED-01: "Kafka projection
    envelopes MUST stamp Timestamp from IClock.UtcNow, not
    consume-moment wall clock." Current code is already compliant.

5. (DEFERRED to B) IProjectionHandler relocation, ModuleCatalogLoader,
   csproj graph surgery — architectural, not bugfix.

6. (DEFERRED to B) IRuntimeControlPlane / ITodoIntentHandler
   indirection — types do not exist; new pattern.

7. TodoProjectionHandler: thread CorrelationId from envelope into the
   SQL upsert. No `IdempotencyKey` field exists in this handler;
   prompt literal was incorrect — only `corrId = Guid.Empty` is
   replaced. Implementation: stash envelope in private field set
   inside the dispatching `HandleAsync(envelope)` (safe under
   ExecutionPolicy.Inline). No public contract changes.

8. Replace 16 `Guid.NewGuid()` call sites in
   tests/unit/operational-system/sandbox/todo/{TodoEngineTests,
   TodoAggregateTests}.cs and tests/integration/eventstore/
   EventOrderingTest.cs with seeded TestIdGenerator calls.

9. (DEFERRED to B) Domain `Adapter*` rename.

10. (DEFERRED to C) New rule promotion.

## OUTPUT FORMAT
Per-step diffs + build result.

## VALIDATION CRITERIA
- `dotnet build Whycespace.sln` green after each step.
- No new guard violations introduced.
- Existing tests still compile.
