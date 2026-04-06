using Whycespace.Shared.Contracts.Systems.Intent;

namespace Whycespace.Systems.Midstream.Incident;

/// <summary>
/// Incident response workflow — composition only.
/// Orchestrates: detect → halt → investigate → resolve → resume.
/// Routes alerts to emergency control and governance automatically.
/// NO execution, NO domain mutation, NO persistence.
/// </summary>
public sealed class IncidentResponseWorkflow
{
    private readonly ISystemIntentDispatcher _intentDispatcher;

    public IncidentResponseWorkflow(ISystemIntentDispatcher intentDispatcher)
    {
        _intentDispatcher = intentDispatcher;
    }

    /// <summary>
    /// Full incident response: detect, halt affected scope, start investigation.
    /// </summary>
    public async Task<IntentResult> TriggerResponseAsync(
        string incidentType, string affectedScope, string affectedRegion,
        string correlationId, CancellationToken ct = default)
    {
        // Step 1: Create incident record
        var detectResult = await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = Guid.Empty,
            CommandType = "operational.incident.response.detect",
            Payload = new { IncidentType = incidentType, AffectedScope = affectedScope, AffectedRegion = affectedRegion },
            CorrelationId = correlationId,
            Timestamp = default,
            Headers = new Dictionary<string, string> { ["x-target-region"] = affectedRegion }
        }, ct);

        if (!detectResult.Success) return detectResult;

        // Step 2: Trigger emergency halt for affected scope
        await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = Guid.Empty,
            CommandType = "operational.deployment.emergency.halt",
            Payload = new { ScopeType = affectedScope, TargetId = affectedRegion, Reason = $"Incident: {incidentType}" },
            CorrelationId = correlationId,
            Timestamp = default,
            Headers = new Dictionary<string, string> { ["x-target-region"] = affectedRegion }
        }, ct);

        // Step 3: Notify governance
        await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = Guid.Empty,
            CommandType = "governance.escalation.incident",
            Payload = new { IncidentType = incidentType, AffectedScope = affectedScope, Region = affectedRegion },
            CorrelationId = correlationId,
            Timestamp = default
        }, ct);

        return detectResult;
    }

    /// <summary>
    /// Resume operations after incident resolution.
    /// </summary>
    public async Task<IntentResult> ResumeAfterResolutionAsync(
        string incidentId, string regionId,
        string correlationId, CancellationToken ct = default)
    {
        // Resolve emergency halt
        await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = Guid.Empty,
            CommandType = "operational.deployment.emergency.resolve",
            Payload = new { TargetId = regionId },
            CorrelationId = correlationId,
            Timestamp = default,
            Headers = new Dictionary<string, string> { ["x-target-region"] = regionId }
        }, ct);

        // Resume region in canary mode
        return await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = Guid.Empty,
            CommandType = "operational.deployment.activation.resume",
            Payload = new { RegionId = regionId },
            CorrelationId = correlationId,
            Timestamp = default,
            Headers = new Dictionary<string, string> { ["x-target-region"] = regionId }
        }, ct);
    }
}
