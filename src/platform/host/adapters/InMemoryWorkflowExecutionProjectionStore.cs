using System.Collections.Concurrent;
using Whycespace.Shared.Contracts.Projections.Orchestration.Workflow;

namespace Whycespace.Platform.Host.Adapters;

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

    public Task<IReadOnlyList<WorkflowExecutionReadModel>> ListByStatusAsync(
        string status,
        int limit = 100,
        CancellationToken cancellationToken = default)
        => ListAsync(status: status, workflowName: null, approvalState: null, limit: limit, cancellationToken);

    public Task<IReadOnlyList<WorkflowExecutionReadModel>> ListAsync(
        string? status = null,
        string? workflowName = null,
        string? approvalState = null,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var effectiveLimit = Math.Min(Math.Max(1, limit), 1000);
        var results = _store.Values
            .Where(m => status is null || string.Equals(m.Status, status, StringComparison.Ordinal))
            .Where(m => workflowName is null || string.Equals(m.WorkflowName, workflowName, StringComparison.Ordinal))
            .Where(m => approvalState is null || string.Equals(m.ApprovalState, approvalState, StringComparison.Ordinal))
            .OrderBy(m => m.WorkflowName, StringComparer.Ordinal)
            .ThenBy(m => m.WorkflowExecutionId)
            .Take(effectiveLimit)
            .Select(m => m with { StepOutputs = new Dictionary<string, object?>(m.StepOutputs) })
            .ToList();
        return Task.FromResult<IReadOnlyList<WorkflowExecutionReadModel>>(results);
    }
}
