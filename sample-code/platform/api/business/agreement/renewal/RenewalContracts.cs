namespace Whycespace.Platform.Api.Business.Agreement.Renewal;

public sealed record RenewalRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record RenewalResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
