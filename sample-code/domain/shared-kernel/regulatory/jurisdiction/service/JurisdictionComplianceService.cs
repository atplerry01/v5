namespace Whycespace.Domain.SharedKernel.Regulatory.Jurisdiction;

public sealed class JurisdictionComplianceService
{
    public bool IsCompliant(JurisdictionAggregate jurisdiction, RegulatoryFramework requiredFramework)
    {
        var spec = new JurisdictionComplianceSpecification(requiredFramework);
        return spec.IsSatisfiedBy(jurisdiction);
    }

    public bool RequiresDataResidency(JurisdictionAggregate jurisdiction)
    {
        return jurisdiction.RequiresLocalStorage();
    }
}
