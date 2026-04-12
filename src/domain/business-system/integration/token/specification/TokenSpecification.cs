namespace Whycespace.Domain.BusinessSystem.Integration.Token;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(TokenStatus status)
    {
        return status == TokenStatus.Issued;
    }
}

public sealed class CanExpireSpecification
{
    public bool IsSatisfiedBy(TokenStatus status)
    {
        return status == TokenStatus.Active;
    }
}

public sealed class CanRevokeSpecification
{
    public bool IsSatisfiedBy(TokenStatus status)
    {
        return status == TokenStatus.Active;
    }
}
