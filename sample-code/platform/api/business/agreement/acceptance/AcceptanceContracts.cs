namespace Whycespace.Platform.Api.Business.Agreement.Acceptance;

public sealed record AcceptanceRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record AcceptanceResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
