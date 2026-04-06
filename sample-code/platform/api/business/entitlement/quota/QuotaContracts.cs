namespace Whycespace.Platform.Api.Business.Entitlement.Quota;

public sealed record QuotaRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record QuotaResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
