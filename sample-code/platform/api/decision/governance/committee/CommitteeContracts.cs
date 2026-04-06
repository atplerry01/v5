namespace Whycespace.Platform.Api.Decision.Governance.Committee;

public sealed record CommitteeRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CommitteeResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
