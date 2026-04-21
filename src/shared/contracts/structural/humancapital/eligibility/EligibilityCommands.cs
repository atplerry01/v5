using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Structural.Humancapital.Eligibility;

public sealed record CreateEligibilityCommand(
    Guid EligibilityId,
    string Name,
    string Kind) : IHasAggregateId
{
    public Guid AggregateId => EligibilityId;
}
