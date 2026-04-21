using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Contract;

public sealed class ContractParty
{
    public PartyId PartyId { get; }
    public string Role { get; }

    public ContractParty(PartyId partyId, string role)
    {
        Guard.Against(partyId == default, "PartyId must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(role), "Role must not be empty.");

        PartyId = partyId;
        Role = role;
    }
}
