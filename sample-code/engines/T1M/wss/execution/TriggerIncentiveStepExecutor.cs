using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T1M.Wss.Execution;

/// <summary>
/// Stateless step executor for incentive triggering.
/// Extracted from HEOS workflow — execution logic belongs in T1M.
/// </summary>
public sealed class TriggerIncentiveStepExecutor
{
    private readonly IRuntimeControlPlane _controlPlane;
    private readonly IClock _clock;
    private readonly IIdGenerator _idGen;

    public TriggerIncentiveStepExecutor(
        IRuntimeControlPlane controlPlane,
        IClock clock,
        IIdGenerator idGen)
    {
        _controlPlane = controlPlane;
        _clock = clock;
        _idGen = idGen;
    }

    public async Task<StepExecutionResult> ExecuteAsync(
        WorkflowExecutionContext context,
        string participantId,
        string incentiveType,
        CancellationToken ct = default)
    {
        var result = await _controlPlane.ExecuteAsync(new RuntimeCommandEnvelope
        {
            CommandId = _idGen.DeterministicGuid($"incentive.trigger:CommandId:{context.CorrelationId}:{participantId}:{incentiveType}"),
            CommandType = "incentive.trigger",
            Payload = new { ParticipantId = participantId, IncentiveType = incentiveType },
            CorrelationId = context.CorrelationId.ToString(),
            Timestamp = _clock.UtcNowOffset
        }, ct);

        return new StepExecutionResult("incentive.trigger", result.Success,
            result.Success ? null : "Incentive trigger failed.");
    }
}
