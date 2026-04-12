namespace Whycespace.Domain.BusinessSystem.Agreement.Contract;

public sealed class ContractParty
{
    public Guid PartyId { get; }
    public string Role { get; }

    public ContractParty(Guid partyId, string role)
    {
        if (partyId == Guid.Empty)
            throw new ArgumentException("PartyId must not be empty.", nameof(partyId));

        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role must not be empty.", nameof(role));

        PartyId = partyId;
        Role = role;
    }
}
