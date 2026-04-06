namespace Whycespace.Platform.Api.Intelligence.Experiment.Variant;

public sealed record VariantRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record VariantResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
