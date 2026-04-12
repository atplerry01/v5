namespace Whycespace.Domain.StructuralSystem.Cluster.Provider;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ProviderStatus status)
    {
        return status == ProviderStatus.Registered;
    }
}

public sealed class CanSuspendSpecification
{
    public bool IsSatisfiedBy(ProviderStatus status)
    {
        return status == ProviderStatus.Active;
    }
}
