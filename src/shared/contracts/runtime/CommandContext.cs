namespace Whyce.Shared.Contracts.Runtime;

public sealed record CommandContext
{
    public required Guid CorrelationId { get; init; }
    public required Guid CausationId { get; init; }
    public required string TenantId { get; init; }
    public required string ActorId { get; init; }
    public required Guid AggregateId { get; init; }
    public required string PolicyId { get; init; }
    public string? PolicyDecisionHash { get; init; }
}
