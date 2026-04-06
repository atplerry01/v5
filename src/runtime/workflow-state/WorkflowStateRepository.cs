using System.Collections.Concurrent;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Runtime.WorkflowState;

public sealed class WorkflowStateRepository : IWorkflowStateRepository
{
    private readonly ConcurrentDictionary<string, WorkflowStateRecord> _store = new();

    public Task SaveAsync(WorkflowStateRecord state)
    {
        if (!_store.TryAdd(state.WorkflowId, state))
        {
            throw new InvalidOperationException(
                $"Workflow state already exists for WorkflowId '{state.WorkflowId}'.");
        }

        return Task.CompletedTask;
    }

    public Task<WorkflowStateRecord?> GetAsync(string workflowId)
    {
        _store.TryGetValue(workflowId, out var state);
        return Task.FromResult(state);
    }

    public Task UpdateAsync(WorkflowStateRecord state)
    {
        if (!_store.ContainsKey(state.WorkflowId))
        {
            throw new InvalidOperationException(
                $"Workflow state not found for WorkflowId '{state.WorkflowId}'.");
        }

        _store[state.WorkflowId] = state;
        return Task.CompletedTask;
    }
}
