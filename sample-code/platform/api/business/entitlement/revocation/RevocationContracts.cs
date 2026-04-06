namespace Whycespace.Platform.Api.Business.Entitlement.Revocation;

public sealed record RevocationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record RevocationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
