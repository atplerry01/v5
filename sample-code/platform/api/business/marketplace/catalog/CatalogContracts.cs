namespace Whycespace.Platform.Api.Business.Marketplace.Catalog;

public sealed record CatalogRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CatalogResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
