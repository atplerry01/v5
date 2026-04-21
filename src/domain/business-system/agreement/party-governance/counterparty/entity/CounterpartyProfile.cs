using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.PartyGovernance.Counterparty;

public sealed class CounterpartyProfile
{
    public Guid IdentityReference { get; }
    public string Name { get; }

    public CounterpartyProfile(Guid identityReference, string name)
    {
        Guard.Against(identityReference == Guid.Empty, "IdentityReference must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(name), "Name must not be empty.");

        IdentityReference = identityReference;
        Name = name;
    }
}
