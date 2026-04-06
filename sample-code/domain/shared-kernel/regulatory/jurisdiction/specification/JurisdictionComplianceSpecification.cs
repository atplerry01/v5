namespace Whycespace.Domain.SharedKernel.Regulatory.Jurisdiction;

public sealed class JurisdictionComplianceSpecification : Specification<JurisdictionAggregate>
{
    private readonly RegulatoryFramework _requiredFramework;

    public JurisdictionComplianceSpecification(RegulatoryFramework requiredFramework)
    {
        _requiredFramework = requiredFramework;
    }

    public override bool IsSatisfiedBy(JurisdictionAggregate entity)
    {
        return entity.RegulatoryFramework == _requiredFramework;
    }
}
