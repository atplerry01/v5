namespace Whycespace.Domain.StructuralSystem.Cluster.Provider;

public readonly record struct ProviderId
{
    public Guid Value { get; }

    public ProviderId(Guid value)
    {
        if (value == Guid.Empty)
            throw ProviderErrors.MissingId();

        Value = value;
    }
}
