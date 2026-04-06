namespace Whyce.Shared.Contracts.Runtime;

public sealed record CommandContext
{
    public required Guid CorrelationId { get; init; }
    public required Guid CausationId { get; init; }
    public required Guid CommandId { get; init; }
    public required string TenantId { get; init; }
    public required string ActorId { get; init; }
    public required Guid AggregateId { get; init; }
    public required string PolicyId { get; init; }
    public string? IdentityId { get; set; }
    public string[]? Roles { get; set; }
    public int? TrustScore { get; set; }
    public bool? PolicyDecisionAllowed { get; set; }
    public string? PolicyDecisionHash { get; set; }
}
