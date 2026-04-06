using Whycespace.Runtime.Command;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Observability;

namespace Whycespace.Runtime.Simulation;

public sealed record SimulationExecutionResult
{
    public required Guid SimulationId { get; init; }
    public required Guid CommandId { get; init; }
    public required bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public required IReadOnlyList<RuntimeEvent> CapturedEvents { get; init; }
    public required IReadOnlyList<TraceSpan> CapturedSpans { get; init; }
    public required IReadOnlyList<string> ExecutionLog { get; init; }
    public required TimeSpan Elapsed { get; init; }
    public CommandResult? ProjectedResult { get; init; }
}
