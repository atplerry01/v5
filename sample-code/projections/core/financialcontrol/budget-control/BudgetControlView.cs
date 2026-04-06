namespace Whycespace.Projections.Core.Financialcontrol.BudgetControl;

public sealed record BudgetControlView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
