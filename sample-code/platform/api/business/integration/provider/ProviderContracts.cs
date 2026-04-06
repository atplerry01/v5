namespace Whycespace.Platform.Api.Business.Integration.Provider;

public sealed record ProviderRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ProviderResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
