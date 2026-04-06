namespace Whycespace.Platform.Api.Trust.Identity.Consent;

public sealed record ConsentRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ConsentResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
