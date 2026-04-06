using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed class IncidentEscalationSpec : Specification<IncidentBaseAggregate>
{
    public override bool IsSatisfiedBy(IncidentBaseAggregate entity)
        => entity.Status.IsActive
        && entity.Severity.CanEscalate;
}
