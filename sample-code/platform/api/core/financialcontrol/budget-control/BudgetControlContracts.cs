namespace Whycespace.Platform.Api.Core.Financialcontrol.BudgetControl;

public sealed record BudgetControlRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record BudgetControlResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
