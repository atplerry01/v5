namespace Whycespace.Projections.Core.Reconciliation.ReconciliationException;

public sealed record ReconciliationExceptionView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
