using Whycespace.Runtime.Command;
using Whycespace.Runtime.Dispatcher;

namespace Whycespace.Runtime.Workflow;

public enum WorkflowStatus
{
    Created,
    Running,
    Completed,
    Faulted
}

public enum StepStatus
{
    Pending,
    Running,
    Completed,
    Faulted
}

public sealed class StepState
{
    public required WorkflowStep Step { get; init; }
    public StepStatus Status { get; set; } = StepStatus.Pending;
    public CommandResult? Result { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}

public sealed class WorkflowInstance
{
    public Guid WorkflowId { get; init; }
    public required CommandContext CommandContext { get; init; }
    public WorkflowStatus Status { get; private set; } = WorkflowStatus.Created;
    public List<StepState> Steps { get; } = new();
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }

    public int CurrentStepIndex { get; private set; }

    public void Initialize(WorkflowStep[] steps)
    {
        if (Steps.Count > 0)
            throw new InvalidOperationException("Workflow already initialized.");

        foreach (var step in steps)
        {
            Steps.Add(new StepState { Step = step });
        }

        Status = WorkflowStatus.Running;
        CreatedAt = CommandContext.Clock.UtcNowOffset;
        CurrentStepIndex = 0;
    }

    public StepState? GetCurrentStep()
    {
        return CurrentStepIndex < Steps.Count ? Steps[CurrentStepIndex] : null;
    }

    public void AdvanceStep()
    {
        CurrentStepIndex++;

        if (CurrentStepIndex >= Steps.Count)
        {
            Status = WorkflowStatus.Completed;
            CompletedAt = CommandContext.Clock.UtcNowOffset;
        }
    }

    public void Fault(string errorMessage)
    {
        Status = WorkflowStatus.Faulted;
        ErrorMessage = errorMessage;
        CompletedAt = CommandContext.Clock.UtcNowOffset;
    }
}
