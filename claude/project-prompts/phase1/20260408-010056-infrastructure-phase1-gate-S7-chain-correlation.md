# TITLE
phase1-gate-S7 — Surface correlation_id on CommandResult to fix chain traceability

# CONTEXT
Phase 1 Hardening, Task 7 + VERDICT.md Drift #5. EventFabric already passes
the SAME context.CorrelationId to ChainAnchorService, EventStoreService,
and OutboxService — chain row, event store row, and outbox row all share
one correlation id. The actual gap is that this id was never returned to
the API caller, so testers/operators couldn't trace HTTP response → chain
row.

After phase1-gate-S1, CreateTodoCommand also flows through the dispatcher
path, so the fix is uniform across all three commands.

Classification: infrastructure / phase1-hardening / chain-traceability
Domain: shared/contracts/runtime, runtime/dispatcher, platform/api

# OBJECTIVE
Add CorrelationId to CommandResult, stamp it in SystemIntentDispatcher,
expose it in API responses.

# CONSTRAINTS
- $5: no architecture changes; existing chain wiring already correct.
- Backwards-compatible: new field defaults to Guid.Empty.

# EXECUTION STEPS
1. CommandResult: add `Guid CorrelationId { get; init; }`.
2. SystemIntentDispatcher: `return result with { CorrelationId = correlationId };`.
3. TodoController.Create: include `correlationId` in success response.

# OUTPUT FORMAT
3 file edits.

# VALIDATION CRITERIA
- dotnet build succeeds
- Existing CommandResult.Success/Failure factory methods unchanged
