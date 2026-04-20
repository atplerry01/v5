namespace Whycespace.Domain.BusinessSystem.Agreement.PartyGovernance.Counterparty;

public sealed class CounterpartyProfile
{
    public Guid IdentityReference { get; }
    public string Name { get; }

    public CounterpartyProfile(Guid identityReference, string name)
    {
        if (identityReference == Guid.Empty)
            throw new ArgumentException("IdentityReference must not be empty.", nameof(identityReference));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name must not be empty.", nameof(name));

        IdentityReference = identityReference;
        Name = name;
    }
}
