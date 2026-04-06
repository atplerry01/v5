namespace Whycespace.Platform.Api.Core.Command.CommandCatalog;

public sealed record CommandCatalogRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CommandCatalogResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
