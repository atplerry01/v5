namespace Whycespace.Shared.Contracts.Policy;

/// <summary>
/// Contract for the enforcement pipeline.
/// Runtime depends on this — implementations live in engine/domain layer.
/// </summary>
public interface IPolicyEnforcementPipeline
{
    Task ExecuteAsync(
        PolicyEvaluationResult result,
        string correlationId,
        CancellationToken cancellationToken = default);
}

public sealed record EnforcementActionDto
{
    public required Guid ActionId { get; init; }
    public required string Type { get; init; }
    public required string Severity { get; init; }
    public required string TargetType { get; init; }
    public required string TargetId { get; init; }
    public required string Reason { get; init; }
    public required string CorrelationId { get; init; }
}
