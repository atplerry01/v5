namespace Whycespace.Platform.Api.Structural.Humancapital.Workforce;

public sealed record WorkforceRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record WorkforceResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
