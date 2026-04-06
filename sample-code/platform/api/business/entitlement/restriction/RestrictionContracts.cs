namespace Whycespace.Platform.Api.Business.Entitlement.Restriction;

public sealed record RestrictionRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record RestrictionResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
