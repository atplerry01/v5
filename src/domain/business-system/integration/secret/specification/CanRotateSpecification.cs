namespace Whycespace.Domain.BusinessSystem.Integration.Secret;

public sealed class CanRotateSpecification
{
    public bool IsSatisfiedBy(SecretStatus status)
    {
        return status == SecretStatus.Active;
    }
}
