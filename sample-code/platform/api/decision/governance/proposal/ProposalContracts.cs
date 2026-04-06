namespace Whycespace.Platform.Api.Decision.Governance.Proposal;

public sealed record ProposalRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ProposalResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
