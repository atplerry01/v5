using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T1M.Wss.Execution;

/// <summary>
/// Stateless step executor for performance evaluation.
/// Extracted from HEOS workflow — execution logic belongs in T1M.
/// </summary>
public sealed class EvaluatePerformanceStepExecutor
{
    private readonly IRuntimeControlPlane _controlPlane;
    private readonly IClock _clock;
    private readonly IIdGenerator _idGen;

    public EvaluatePerformanceStepExecutor(
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
        string evaluationPeriod,
        CancellationToken ct = default)
    {
        var result = await _controlPlane.ExecuteAsync(new RuntimeCommandEnvelope
        {
            CommandId = _idGen.DeterministicGuid($"performance.evaluate:CommandId:{context.CorrelationId}:{participantId}:{evaluationPeriod}"),
            CommandType = "performance.evaluate",
            Payload = new { ParticipantId = participantId, EvaluationPeriod = evaluationPeriod },
            CorrelationId = context.CorrelationId.ToString(),
            Timestamp = _clock.UtcNowOffset
        }, ct);

        return new StepExecutionResult("performance.evaluate", result.Success,
            result.Success ? null : "Performance evaluation failed.");
    }
}
