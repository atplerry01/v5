namespace Whycespace.Shared.Contracts.Economic.Routing.Execution;

public sealed record RoutingExecutionReadModel
{
    public Guid ExecutionId { get; init; }
    public Guid PathId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset? TerminalAt { get; init; }
    public string TerminalReason { get; init; } = string.Empty;
}
