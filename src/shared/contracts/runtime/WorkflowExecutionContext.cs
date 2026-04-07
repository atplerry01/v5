namespace Whyce.Shared.Contracts.Runtime;

public sealed class WorkflowExecutionContext : IDomainEventSink
{
    public required Guid WorkflowId { get; init; }
    public required Guid CorrelationId { get; init; }
    public required string WorkflowName { get; init; }
    public required object Payload { get; init; }
    public int CurrentStepIndex { get; set; }
    public string CurrentStep { get; set; } = string.Empty;
    public Dictionary<string, string> State { get; } = new();
    public Dictionary<string, object?> StepOutputs { get; } = new();
    public List<object> AccumulatedEvents { get; } = new();
    public object? WorkflowOutput { get; set; }
    public string ExecutionHash { get; set; } = string.Empty;
    public string? IdentityId { get; set; }
    public string? PolicyDecision { get; set; }

    public IReadOnlyList<object> EmittedEvents => AccumulatedEvents.AsReadOnly();

    public void EmitEvent(object domainEvent) => AccumulatedEvents.Add(domainEvent);

    public void EmitEvents(IEnumerable<object> domainEvents) => AccumulatedEvents.AddRange(domainEvents);
}
