namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public static class CanInvestigateSpecification
{
    public static bool IsSatisfiedBy(IncidentStatus status) => status == IncidentStatus.Reported;
}

public static class CanResolveSpecification
{
    public static bool IsSatisfiedBy(IncidentStatus status) => status == IncidentStatus.Investigating;
}

public static class CanCloseSpecification
{
    public static bool IsSatisfiedBy(IncidentStatus status) => status == IncidentStatus.Resolved;
}
