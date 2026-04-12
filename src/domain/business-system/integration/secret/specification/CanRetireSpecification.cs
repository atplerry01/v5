namespace Whycespace.Domain.BusinessSystem.Integration.Secret;

public sealed class CanRetireSpecification
{
    public bool IsSatisfiedBy(SecretStatus status)
    {
        return status == SecretStatus.Active || status == SecretStatus.Rotated;
    }
}
