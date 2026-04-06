using Whycespace.Shared.Contracts.Systems.Intent;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Systems.Upstream.WhycePolicy.Simulation;

public sealed class PolicySimulationService
{
    private readonly ISystemIntentDispatcher _intentDispatcher;
    private readonly IClock _clock;

    public PolicySimulationService(ISystemIntentDispatcher intentDispatcher, IClock clock)
    {
        _intentDispatcher = intentDispatcher;
        _clock = clock;
    }

    public async Task<PolicySimulationResult> SimulateAsync(
        string policyId,
        IReadOnlyDictionary<string, object> inputs,
        CancellationToken cancellationToken = default)
    {
        var result = await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = DeterministicIdHelper.FromSeed($"PolicySimulationService:SimulateAsync:{policyId}"),
            CommandType = "policy.simulate",
            Payload = new { PolicyId = policyId, Inputs = inputs },
            CorrelationId = DeterministicIdHelper.FromSeed($"correlation:PolicySimulationService:{policyId}").ToString(),
            Timestamp = _clock.UtcNowOffset,
            PolicyId = policyId
        }, cancellationToken);

        return new PolicySimulationResult
        {
            PolicyId = policyId,
            WouldPass = result.Success,
            SimulatedOutcome = result.Data,
            Reason = result.ErrorMessage
        };
    }
}

public sealed record PolicySimulationResult
{
    public required string PolicyId { get; init; }
    public required bool WouldPass { get; init; }
    public object? SimulatedOutcome { get; init; }
    public string? Reason { get; init; }
}
