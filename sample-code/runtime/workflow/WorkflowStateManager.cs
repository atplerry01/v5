using System.Collections.Concurrent;
using Whycespace.Runtime.Workflow.State;

namespace Whycespace.Runtime.Workflow;

public sealed class WorkflowStateManager
{
    private readonly ConcurrentDictionary<Guid, WorkflowInstance> _instances = new();
    private readonly IWorkflowStateStore? _stateStore;
    private readonly ConcurrentDictionary<Guid, long> _versions = new();

    public WorkflowStateManager() { }

    public WorkflowStateManager(IWorkflowStateStore stateStore)
    {
        _stateStore = stateStore;
    }

    public async Task TrackAsync(WorkflowInstance instance, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(instance);

        if (!_instances.TryAdd(instance.WorkflowId, instance))
        {
            throw new InvalidOperationException(
                $"Workflow {instance.WorkflowId} is already tracked.");
        }

        _versions[instance.WorkflowId] = 1;

        if (_stateStore is not null)
            await _stateStore.SaveAsync(CreateSnapshot(instance), cancellationToken);
    }

    public WorkflowInstance? Get(Guid workflowId)
    {
        _instances.TryGetValue(workflowId, out var instance);
        return instance;
    }

    public async Task MarkStepRunningAsync(WorkflowInstance instance, CancellationToken cancellationToken = default)
    {
        var step = instance.GetCurrentStep();
        if (step is null) return;

        step.Status = StepStatus.Running;
        step.StartedAt = instance.CommandContext.Clock.UtcNowOffset;

        await PersistAsync(instance, cancellationToken);
    }

    public async Task MarkStepCompletedAsync(WorkflowInstance instance, Command.CommandResult result, CancellationToken cancellationToken = default)
    {
        var step = instance.GetCurrentStep();
        if (step is null) return;

        step.Status = result.Success ? StepStatus.Completed : StepStatus.Faulted;
        step.Result = result;
        step.CompletedAt = instance.CommandContext.Clock.UtcNowOffset;

        await PersistAsync(instance, cancellationToken);
    }

    public async Task RemoveAsync(Guid workflowId, CancellationToken cancellationToken = default)
    {
        _instances.TryRemove(workflowId, out _);
        _versions.TryRemove(workflowId, out _);

        if (_stateStore is not null)
            await _stateStore.DeleteAsync(workflowId, cancellationToken);
    }

    private async Task PersistAsync(WorkflowInstance instance, CancellationToken cancellationToken)
    {
        if (_stateStore is null) return;

        var version = _versions.AddOrUpdate(instance.WorkflowId, 1, (_, v) => v + 1);
        await _stateStore.SaveAsync(CreateSnapshot(instance, version), cancellationToken);
    }

    private WorkflowStateSnapshot CreateSnapshot(WorkflowInstance instance, long? version = null)
    {
        var v = version ?? _versions.GetOrAdd(instance.WorkflowId, 1);

        return new WorkflowStateSnapshot
        {
            WorkflowId = instance.WorkflowId,
            CommandType = instance.CommandContext.Envelope.CommandType,
            CorrelationId = instance.CommandContext.Envelope.CorrelationId,
            ExecutionId = instance.CommandContext.ExecutionId,
            Status = instance.Status,
            CurrentStepIndex = instance.CurrentStepIndex,
            TotalSteps = instance.Steps.Count,
            Version = v,
            Steps = instance.Steps.Select(s => new StepSnapshot
            {
                EngineCommandType = s.Step.EngineCommandType,
                Status = s.Status,
                StartedAt = s.StartedAt,
                CompletedAt = s.CompletedAt,
                Success = s.Result?.Success ?? false,
                ErrorMessage = s.Result?.ErrorMessage
            }).ToList(),
            CreatedAt = instance.CreatedAt,
            CompletedAt = instance.CompletedAt,
            ErrorMessage = instance.ErrorMessage
        };
    }
}
