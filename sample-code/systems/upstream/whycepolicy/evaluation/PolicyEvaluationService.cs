using Whycespace.Shared.Contracts.Systems.Intent;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Systems.Upstream.WhycePolicy.Evaluation;

public sealed class PolicyEvaluationService
{
    private readonly ISystemIntentDispatcher _intentDispatcher;
    private readonly IClock _clock;

    public PolicyEvaluationService(ISystemIntentDispatcher intentDispatcher, IClock clock)
    {
        _intentDispatcher = intentDispatcher;
        _clock = clock;
    }

    public async Task<PolicyEvaluationResult> EvaluateAsync(
        string policyId,
        IReadOnlyDictionary<string, object> facts,
        CancellationToken cancellationToken = default)
    {
        var result = await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = DeterministicIdHelper.FromSeed($"PolicyEvaluationService:EvaluateAsync:{policyId}"),
            CommandType = "policy.evaluate-rules",
            Payload = new { PolicyId = policyId, Facts = facts },
            CorrelationId = DeterministicIdHelper.FromSeed($"correlation:PolicyEvaluationService:{policyId}").ToString(),
            Timestamp = _clock.UtcNowOffset,
            PolicyId = policyId
        }, cancellationToken);

        return new PolicyEvaluationResult
        {
            PolicyId = policyId,
            Passed = result.Success,
            Reason = result.Success ? null : result.ErrorMessage
        };
    }
}

public sealed record PolicyEvaluationResult
{
    public required string PolicyId { get; init; }
    public required bool Passed { get; init; }
    public string? Reason { get; init; }
}
