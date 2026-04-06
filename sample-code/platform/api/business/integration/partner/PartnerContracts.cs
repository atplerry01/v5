namespace Whycespace.Platform.Api.Business.Integration.Partner;

public sealed record PartnerRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record PartnerResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
