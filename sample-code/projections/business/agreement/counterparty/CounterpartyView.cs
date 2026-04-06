namespace Whycespace.Projections.Business.Agreement.Counterparty;

public sealed record CounterpartyView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
