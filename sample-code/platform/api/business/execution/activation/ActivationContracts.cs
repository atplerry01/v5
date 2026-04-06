namespace Whycespace.Platform.Api.Business.Execution.Activation;

public sealed record ActivationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ActivationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
