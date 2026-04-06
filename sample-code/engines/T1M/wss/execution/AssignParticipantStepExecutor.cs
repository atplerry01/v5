using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T1M.Wss.Execution;

/// <summary>
/// Stateless step executor for workforce participant assignment.
/// Extracted from HEOS workflow — execution logic belongs in T1M.
/// </summary>
public sealed class AssignParticipantStepExecutor
{
    private readonly IRuntimeControlPlane _controlPlane;
    private readonly IClock _clock;
    private readonly IIdGenerator _idGen;

    public AssignParticipantStepExecutor(
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
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken ct = default)
    {
        var result = await _controlPlane.ExecuteAsync(new RuntimeCommandEnvelope
        {
            CommandId = _idGen.DeterministicGuid($"workforce.assign-participant:CommandId:{context.CorrelationId}:{parameters["participantId"]}:{parameters["workforceId"]}"),
            CommandType = "workforce.assign-participant",
            Payload = new
            {
                ParticipantId = parameters["participantId"],
                WorkforceId = parameters["workforceId"]
            },
            CorrelationId = context.CorrelationId.ToString(),
            Timestamp = _clock.UtcNowOffset
        }, ct);

        return new StepExecutionResult("workforce.assign-participant", result.Success,
            result.Success ? null : "Failed to assign participant to workforce.");
    }
}
