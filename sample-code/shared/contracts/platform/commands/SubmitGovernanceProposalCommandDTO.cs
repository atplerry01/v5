namespace Whycespace.Shared.Contracts.Platform.Commands;

/// <summary>
/// Flattened command DTO for submitting a governance proposal.
/// No domain entities exposed — primitives only.
/// </summary>
public sealed record SubmitGovernanceProposalCommandDTO
{
    public required string CommandId { get; init; }
    public required int Version { get; init; }
    public required DateTimeOffset Timestamp { get; init; }

    public required Guid ProposerIdentityId { get; init; }
    public required string ProposalTitle { get; init; }
    public required string ProposalDescription { get; init; }
    public required string ProposalType { get; init; }
    public string? TargetSubjectId { get; init; }
    public string? TargetSubjectType { get; init; }
    public string? CorrelationId { get; init; }
    public string? IdempotencyKey { get; init; }
}
