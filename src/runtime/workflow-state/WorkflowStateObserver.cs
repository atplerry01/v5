using Whyce.Shared.Contracts.Runtime;
using Whyce.Shared.Kernel.Domain;

namespace Whyce.Runtime.WorkflowState;

public sealed class WorkflowStateObserver : IWorkflowStepObserver
{
    private readonly IWorkflowStateRepository _repository;
    private readonly IClock _clock;

    public WorkflowStateObserver(IWorkflowStateRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task OnStepCompletedAsync(WorkflowExecutionContext context, int stepIndex, string stepName)
    {
        var state = await _repository.GetAsync(context.WorkflowId.ToString());
        if (state is null)
        {
            return;
        }

        state.CurrentStepIndex = stepIndex;
        state.ExecutionHash = context.ExecutionHash;
        state.SerializedState = WorkflowStateSerializer.Serialize(context.State);
        state.Status = "Running";
        state.UpdatedAt = _clock.UtcNow;

        await _repository.UpdateAsync(state);
    }

    public async Task OnWorkflowCompletedAsync(WorkflowExecutionContext context)
    {
        var state = await _repository.GetAsync(context.WorkflowId.ToString());
        if (state is null)
        {
            return;
        }

        state.Status = "Completed";
        state.ExecutionHash = context.ExecutionHash;
        state.SerializedState = WorkflowStateSerializer.Serialize(context.State);
        state.UpdatedAt = _clock.UtcNow;

        await _repository.UpdateAsync(state);
    }

    public async Task OnWorkflowFailedAsync(WorkflowExecutionContext context, string failedStep, string error)
    {
        var state = await _repository.GetAsync(context.WorkflowId.ToString());
        if (state is null)
        {
            return;
        }

        state.Status = "Failed";
        state.ExecutionHash = context.ExecutionHash;
        state.SerializedState = WorkflowStateSerializer.Serialize(context.State);
        state.UpdatedAt = _clock.UtcNow;

        await _repository.UpdateAsync(state);
    }
}
