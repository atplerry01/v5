using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Subject.Subject;

public sealed record RegisterEconomicSubjectCommand(
    Guid SubjectId,
    string SubjectType,
    string StructuralRefType,
    string StructuralRefId,
    string EconomicRefType,
    string EconomicRefId) : IHasAggregateId
{
    public Guid AggregateId => SubjectId;
}
