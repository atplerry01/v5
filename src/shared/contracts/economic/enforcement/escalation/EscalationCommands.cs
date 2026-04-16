using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Enforcement.Escalation;

public sealed record AccumulateViolationCommand(
    Guid SubjectId,
    Guid ViolationId,
    string Severity,
    DateTimeOffset OccurredAt) : IHasAggregateId
{
    public Guid AggregateId => SubjectId;
}
