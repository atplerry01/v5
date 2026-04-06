using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed class IncidentClosureSpec : Specification<IncidentBaseAggregate>
{
    public override bool IsSatisfiedBy(IncidentBaseAggregate entity)
        => entity.Status == IncidentStatus.Resolved;
}
