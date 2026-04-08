CLASSIFICATION: orchestration-system / workflow / resume
SOURCE: 20260407-230000-runtime-workflow-resume-execution.md
SEVERITY: S2 — MEDIUM (correctness gap + test gap)

DESCRIPTION:

Two follow-ups discovered while implementing workflow resume:

1. **PAYLOAD-ON-RESUME**: `WorkflowExecutionStartedEvent` records only
   `WorkflowName` and `AggregateId`. The original `Payload` from
   `WorkflowStartCommand.Payload` is NOT persisted to the event store.
   Therefore `WorkflowResumeCommand` cannot reconstruct the original
   payload, and the dispatcher passes `new object()` as a placeholder.
   Workflows whose steps read `context.Payload` (any non-stateless step)
   will silently misbehave on resume.

   Step outputs (`context.StepOutputs`) face the same problem — they are
   not persisted, so steps that read prior step outputs at runtime cannot
   be resumed correctly.

2. **T1M-RESUME-TEST-COVERAGE-01**: Workflow happy-path tests do not
   exist in `tests/unit/` or `tests/integration/`. Only `NoOpWorkflowEngine`
   stubs exist. Resume cannot be tested in isolation without first building
   a T1M test harness (real `WorkflowStepExecutor`, real `WorkflowRegistry`,
   in-memory event store, ≥2 stateless test steps).

PROPOSED_RULE / REMEDIATION:

For (1) — choose ONE of:
  (a) Persist `Payload` (serialized) on `WorkflowExecutionStartedEvent`,
      and restore it in the dispatcher resume path.
  (b) Persist `StepOutputs` snapshots on each `WorkflowStepCompletedEvent`.
  (c) Restrict `WorkflowResumeCommand` to workflows whose steps are
      certified payload-free, enforced by a step interface marker.

  Recommendation: (a) — minimal surface change, restores parity with
  start path. Document that reasonable "resume safety" requires payload
  serialization to be deterministic (already required by $9).

For (2):
  Add a `T1MWorkflowHarness` test fixture under
  `tests/integration/orchestration-system/workflow/` that wires
  `T1MWorkflowEngine`, `WorkflowStepExecutor`, an in-memory
  `IWorkflowRegistry`, and a real `IEventStore`. Then add the three
  scenarios from the pasted H8 prompt:
    - Resume midway (cursor = 2 of 4) → executes steps 2,3
    - Resume completed → fails with "not in resumable state"
    - Resume after failure → re-runs the failed step (or per chosen policy)

UPDATE 2026-04-08 (H9):
  Item (1) PAYLOAD-ON-RESUME is partially closed by H9 (20260408-000000):
  Payload and step outputs are now persisted on
  WorkflowExecutionStartedEvent.Payload and WorkflowStepCompletedEvent.Output.
  REMAINING GAP — TYPED PAYLOAD DESERIALIZATION: PostgresEventStoreAdapter
  serializes events via JsonSerializer.Serialize(evt, evt.GetType()) and
  EventDeserializer.DeserializeStored() returns concrete event types. Because
  the Payload and Output fields are statically typed as `object?` on the
  record, System.Text.Json round-trips them as JsonElement on Postgres-backed
  replay. In-process / in-memory replay preserves the original CLR type by
  reference. Workflow steps that cast `(MyPayloadType)context.Payload` will
  fail on Postgres-backed resume.
  REMEDIATION (proposed): introduce a payload-type registry (event-type →
  payload CLR type) consulted by EventDeserializer when materializing Payload
  and Output. Alternatively, replace `object? Payload` with a typed envelope
  per workflow (heavier; per-workflow domain modeling).

  Item (2) T1M-RESUME-TEST-COVERAGE-01 is UNCHANGED by H9. The test harness
  has not been built; resume is still validated only by build + manual review.

TRACKED BY: (not yet promoted)
