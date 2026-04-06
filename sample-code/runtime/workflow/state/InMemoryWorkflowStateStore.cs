using System.Collections.Concurrent;

namespace Whycespace.Runtime.Workflow.State;

public sealed class InMemoryWorkflowStateStore : IWorkflowStateStore
{
    private readonly ConcurrentDictionary<Guid, WorkflowStateSnapshot> _store = new();

    public Task SaveAsync(WorkflowStateSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        _store.AddOrUpdate(
            snapshot.WorkflowId,
            snapshot,
            (_, existing) =>
            {
                if (snapshot.Version <= existing.Version)
                    throw new InvalidOperationException(
                        $"Optimistic concurrency violation for workflow {snapshot.WorkflowId}. "
                        + $"Expected version > {existing.Version}, got {snapshot.Version}.");
                return snapshot;
            });

        return Task.CompletedTask;
    }

    public Task<WorkflowStateSnapshot?> GetAsync(Guid workflowId, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(workflowId, out var snapshot);
        return Task.FromResult(snapshot);
    }

    public Task<IReadOnlyList<WorkflowStateSnapshot>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var active = _store.Values
            .Where(s => s.Status is WorkflowStatus.Running or WorkflowStatus.Created)
            .ToList();

        return Task.FromResult<IReadOnlyList<WorkflowStateSnapshot>>(active);
    }

    public Task DeleteAsync(Guid workflowId, CancellationToken cancellationToken = default)
    {
        _store.TryRemove(workflowId, out _);
        return Task.CompletedTask;
    }
}
