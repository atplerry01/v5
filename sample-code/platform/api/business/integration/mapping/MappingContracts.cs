namespace Whycespace.Platform.Api.Business.Integration.Mapping;

public sealed record MappingRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record MappingResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
