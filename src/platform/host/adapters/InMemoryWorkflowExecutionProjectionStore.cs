using System.Collections.Concurrent;
using Whyce.Shared.Contracts.Projections.OrchestrationSystem.Workflow;

namespace Whyce.Platform.Host.Adapters;

/// <summary>
/// In-memory placeholder for the workflow execution projection read store.
///
/// PLACEHOLDER (T-PLACEHOLDER-01): the canonical Postgres-backed implementation
/// is tracked at scripts/migrations/002_create_workflow_execution_projection.sql.
/// Swap to a Postgres adapter when the table is provisioned in the projections DB.
/// </summary>
public sealed class InMemoryWorkflowExecutionProjectionStore : IWorkflowExecutionProjectionStore
{
    private readonly ConcurrentDictionary<Guid, WorkflowExecutionReadModel> _store = new();

    public Task<WorkflowExecutionReadModel?> GetAsync(Guid workflowExecutionId)
    {
        if (!_store.TryGetValue(workflowExecutionId, out var model))
            return Task.FromResult<WorkflowExecutionReadModel?>(null);

        // phase1-gate-projection-hardening: defensive copy. ConcurrentDictionary
        // hands back the stored reference; without this copy, callers could
        // mutate StepOutputs via the returned reference and that change would
        // land in the store directly, before any UpsertAsync. The future
        // Postgres adapter will return fresh deserialized records per call,
        // so this copy preserves the same value-semantics contract across
        // both adapters.
        var copy = model with
        {
            StepOutputs = new Dictionary<string, object?>(model.StepOutputs)
        };
        return Task.FromResult<WorkflowExecutionReadModel?>(copy);
    }

    public Task UpsertAsync(WorkflowExecutionReadModel model)
    {
        // Symmetrical defensive copy on the write side: a caller that retains
        // the model reference and mutates its dictionary after UpsertAsync
        // must NOT be able to corrupt stored state.
        var stored = model with
        {
            StepOutputs = new Dictionary<string, object?>(model.StepOutputs)
        };
        _store[model.WorkflowExecutionId] = stored;
        return Task.CompletedTask;
    }
}
