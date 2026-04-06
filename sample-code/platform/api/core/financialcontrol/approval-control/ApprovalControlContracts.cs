namespace Whycespace.Platform.Api.Core.Financialcontrol.ApprovalControl;

public sealed record ApprovalControlRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ApprovalControlResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
