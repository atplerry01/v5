using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T1M.Wss.Execution;

/// <summary>
/// Stateless step executor for workforce task assignment.
/// Extracted from HEOS AssignWorkforceToTaskWorkflow — multi-step execution logic belongs in T1M.
/// Each step is dispatched individually via the runtime control plane.
/// </summary>
public sealed class AssignWorkforceToTaskStepExecutor
{
    private readonly IRuntimeControlPlane _controlPlane;
    private readonly IClock _clock;
    private readonly IIdGenerator _idGen;

    public AssignWorkforceToTaskStepExecutor(
        IRuntimeControlPlane controlPlane,
        IClock clock,
        IIdGenerator idGen)
    {
        _controlPlane = controlPlane;
        _clock = clock;
        _idGen = idGen;
    }

    public async Task<StepExecutionResult> ExecuteValidateEligibilityAsync(
        WorkflowExecutionContext context,
        string participantId,
        CancellationToken ct = default)
    {
        var result = await _controlPlane.ExecuteAsync(new RuntimeCommandEnvelope
        {
            CommandId = _idGen.DeterministicGuid($"identity.validate-eligibility:CommandId:{context.CorrelationId}:{participantId}"),
            CommandType = "identity.validate-eligibility",
            Payload = new { ParticipantId = participantId },
            CorrelationId = context.CorrelationId.ToString(),
            Timestamp = _clock.UtcNowOffset
        }, ct);

        return new StepExecutionResult("identity.validate-eligibility", result.Success,
            result.Success ? null : "Participant eligibility validation failed.");
    }

    public async Task<StepExecutionResult> ExecutePolicyCheckAsync(
        WorkflowExecutionContext context,
        string policyId,
        string participantId,
        string taskId,
        CancellationToken ct = default)
    {
        var result = await _controlPlane.ExecuteAsync(new RuntimeCommandEnvelope
        {
            CommandId = _idGen.DeterministicGuid($"policy.evaluate:CommandId:{context.CorrelationId}:{policyId}:{participantId}:{taskId}"),
            CommandType = "policy.evaluate",
            Payload = new { PolicyId = policyId, ParticipantId = participantId, TaskId = taskId },
            CorrelationId = context.CorrelationId.ToString(),
            Timestamp = _clock.UtcNowOffset,
            PolicyId = policyId
        }, ct);

        return new StepExecutionResult("policy.evaluate", result.Success,
            result.Success ? null : "Policy evaluation failed.");
    }

    public async Task<StepExecutionResult> ExecuteAssignTaskAsync(
        WorkflowExecutionContext context,
        string workforceId,
        string taskId,
        string participantId,
        CancellationToken ct = default)
    {
        var result = await _controlPlane.ExecuteAsync(new RuntimeCommandEnvelope
        {
            CommandId = _idGen.DeterministicGuid($"workforce.assign-task:CommandId:{context.CorrelationId}:{workforceId}:{taskId}:{participantId}"),
            CommandType = "workforce.assign-task",
            Payload = new { WorkforceId = workforceId, TaskId = taskId, ParticipantId = participantId },
            CorrelationId = context.CorrelationId.ToString(),
            Timestamp = _clock.UtcNowOffset
        }, ct);

        return new StepExecutionResult("workforce.assign-task", result.Success,
            result.Success ? null : "Workforce task assignment failed.");
    }

    public async Task<StepExecutionResult> ExecuteEmitAssignmentAsync(
        WorkflowExecutionContext context,
        string workforceId,
        string taskId,
        string participantId,
        CancellationToken ct = default)
    {
        var result = await _controlPlane.ExecuteAsync(new RuntimeCommandEnvelope
        {
            CommandId = _idGen.DeterministicGuid($"event.emit-assignment:CommandId:{context.CorrelationId}:{workforceId}:{taskId}:{participantId}"),
            CommandType = "event.emit-assignment",
            Payload = new { WorkforceId = workforceId, TaskId = taskId, ParticipantId = participantId },
            CorrelationId = context.CorrelationId.ToString(),
            Timestamp = _clock.UtcNowOffset
        }, ct);

        return new StepExecutionResult("event.emit-assignment", result.Success,
            result.Success ? null : "Assignment event emission failed.");
    }
}
