namespace Whycespace.Platform.Api.Business.Portfolio.Mandate;

public sealed record MandateRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record MandateResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
