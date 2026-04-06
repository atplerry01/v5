namespace Whycespace.Platform.Api.Structural.Humancapital.Sanction;

public sealed record SanctionRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record SanctionResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
