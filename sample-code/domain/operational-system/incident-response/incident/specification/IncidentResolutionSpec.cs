using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed class IncidentResolutionSpec : Specification<IncidentBaseAggregate>
{
    public override bool IsSatisfiedBy(IncidentBaseAggregate entity)
        => entity.Status.IsActive
        && entity.CurrentAssignment is not null;
}
