namespace Whycespace.Platform.Api.Governance.Policy.Contracts;

public sealed record SubmitPolicyProposalRequest
{
    public required string PolicyId { get; init; }
    public required string ProposedBy { get; init; }
    public required string DslContent { get; init; }
    public string? CommandId { get; init; }
    public DateTimeOffset? Timestamp { get; init; }
}

public sealed record ApprovePolicyRequest
{
    public required string ProposalId { get; init; }
    public required string ApproverId { get; init; }
    public string Decision { get; init; } = "Approve";
    public string? CommandId { get; init; }
    public DateTimeOffset? Timestamp { get; init; }
}

public sealed record ActivatePolicyRequest
{
    public required string ProposalId { get; init; }
    public string? CommandId { get; init; }
    public DateTimeOffset? Timestamp { get; init; }
}
