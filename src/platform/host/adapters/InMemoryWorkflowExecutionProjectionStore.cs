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
        _store.TryGetValue(workflowExecutionId, out var model);
        return Task.FromResult(model);
    }

    public Task UpsertAsync(WorkflowExecutionReadModel model)
    {
        _store[model.WorkflowExecutionId] = model;
        return Task.CompletedTask;
    }
}
