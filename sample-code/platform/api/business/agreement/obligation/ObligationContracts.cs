namespace Whycespace.Platform.Api.Business.Agreement.Obligation;

public sealed record ObligationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ObligationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
