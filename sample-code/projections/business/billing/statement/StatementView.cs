namespace Whycespace.Projections.Business.Billing.Statement;

public sealed record StatementView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
