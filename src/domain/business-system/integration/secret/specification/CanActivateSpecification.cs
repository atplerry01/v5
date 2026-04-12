namespace Whycespace.Domain.BusinessSystem.Integration.Secret;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(SecretStatus status)
    {
        return status == SecretStatus.Stored;
    }
}
