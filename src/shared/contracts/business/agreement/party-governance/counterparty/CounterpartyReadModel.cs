namespace Whycespace.Shared.Contracts.Business.Agreement.PartyGovernance.Counterparty;

public sealed record CounterpartyReadModel
{
    public Guid CounterpartyId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
