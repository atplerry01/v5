namespace Whycespace.Projections.Core.Reconciliation.ReconciliationItem;

public sealed record ReconciliationItemView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
