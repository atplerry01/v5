namespace Whycespace.Platform.Api.Core.Contracts.Simulation;

/// <summary>
/// Inbound simulation request from the platform layer.
/// Captures the workflow to simulate, the payload, and identity context.
/// Read-only — MUST NOT trigger any state mutation or workflow execution.
/// </summary>
public sealed record SimulationRequest
{
    public required string WorkflowKey { get; init; }
    public required object Payload { get; init; }
    public required Guid IdentityId { get; init; }
    public required string CorrelationId { get; init; }
}