namespace Whycespace.Domain.BusinessSystem.Integration.Partner;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(PartnerStatus status)
    {
        return status == PartnerStatus.Registered;
    }
}

public sealed class CanSuspendSpecification
{
    public bool IsSatisfiedBy(PartnerStatus status)
    {
        return status == PartnerStatus.Active;
    }
}

public sealed class CanDeregisterSpecification
{
    public bool IsSatisfiedBy(PartnerStatus status)
    {
        return status == PartnerStatus.Active || status == PartnerStatus.Suspended;
    }
}
