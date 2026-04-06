namespace Whycespace.Platform.Api.Core.Financialcontrol.VarianceControl;

public sealed record VarianceControlRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record VarianceControlResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
