namespace Whycespace.Platform.Api.Business.Entitlement.EntitlementGrant;

public sealed record EntitlementGrantRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record EntitlementGrantResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
