namespace Whycespace.Platform.Api.Core.Financialcontrol.ReserveControl;

public sealed record ReserveControlRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ReserveControlResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
