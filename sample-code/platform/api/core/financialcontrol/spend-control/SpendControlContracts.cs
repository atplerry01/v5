namespace Whycespace.Platform.Api.Core.Financialcontrol.SpendControl;

public sealed record SpendControlRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record SpendControlResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
