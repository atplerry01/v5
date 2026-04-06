using Whycespace.Shared.Contracts.Systems.Intent;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Systems.Midstream.WhyceAtlas.Simulation;

/// <summary>
/// Isolated execution context for simulation.
/// Records all intents that WOULD be dispatched without mutating real state.
/// Implements ISystemIntentDispatcher so simulation workflows use the same contract.
/// </summary>
public sealed class SimulationSandbox : ISystemIntentDispatcher
{
    private readonly IClock _clock;
    private readonly List<SimulatedExecution> _executions = [];

    public SimulationSandbox(IClock clock)
    {
        _clock = clock;
    }

    public IReadOnlyList<SimulatedExecution> Executions => _executions.AsReadOnly();

    public Task<IntentResult> DispatchAsync(ExecuteCommandIntent intent, CancellationToken cancellationToken = default)
    {
        // Record the intent but do NOT execute it against real infrastructure
        _executions.Add(new SimulatedExecution
        {
            CommandId = intent.CommandId,
            CommandType = intent.CommandType,
            Payload = intent.Payload,
            CorrelationId = intent.CorrelationId,
            SimulatedAt = _clock.UtcNowOffset
        });

        // Return simulated success — no real side effects
        return Task.FromResult(IntentResult.Ok(intent.CommandId, new { Simulated = true }));
    }

    public void Reset() => _executions.Clear();
}

public sealed record SimulatedExecution
{
    public required Guid CommandId { get; init; }
    public required string CommandType { get; init; }
    public required object Payload { get; init; }
    public required string CorrelationId { get; init; }
    public required DateTimeOffset SimulatedAt { get; init; }
}
