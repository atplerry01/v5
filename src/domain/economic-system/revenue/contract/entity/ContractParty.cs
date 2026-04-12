using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Contract;

public sealed class ContractParty : Entity
{
    public Guid PartyId { get; private set; }
    public decimal SharePercentage { get; private set; }

    private ContractParty() { }

    internal static ContractParty Create(Guid partyId, decimal sharePercentage)
    {
        return new ContractParty
        {
            PartyId = partyId,
            SharePercentage = sharePercentage
        };
    }
}
