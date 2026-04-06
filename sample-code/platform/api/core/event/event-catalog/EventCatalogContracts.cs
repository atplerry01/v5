namespace Whycespace.Platform.Api.Core.Event.EventCatalog;

public sealed record EventCatalogRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record EventCatalogResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
