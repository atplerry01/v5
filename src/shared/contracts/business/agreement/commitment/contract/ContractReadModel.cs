namespace Whycespace.Shared.Contracts.Business.Agreement.Commitment.Contract;

public sealed record ContractReadModel
{
    public Guid ContractId { get; init; }
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<ContractPartyReadModel> Parties { get; init; } = Array.Empty<ContractPartyReadModel>();
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}

public sealed record ContractPartyReadModel(Guid PartyId, string Role);
