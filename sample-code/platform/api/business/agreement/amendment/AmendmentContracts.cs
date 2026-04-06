namespace Whycespace.Platform.Api.Business.Agreement.Amendment;

public sealed record AmendmentRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record AmendmentResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
