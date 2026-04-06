namespace Whycespace.Platform.Api.Decision.Governance.Appeal;

public sealed record AppealRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record AppealResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
