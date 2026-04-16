using Whycespace.Shared.Contracts.Economic.Subject.Subject;
using Whycespace.Shared.Contracts.Events.Economic.Subject.Subject;

namespace Whycespace.Projections.Economic.Subject.Subject.Reducer;

public static class SubjectProjectionReducer
{
    public static EconomicSubjectReadModel Apply(EconomicSubjectReadModel state, EconomicSubjectRegisteredEventSchema e) =>
        state with
        {
            SubjectId         = e.AggregateId,
            SubjectType       = e.SubjectType,
            StructuralRefType = e.StructuralRefType,
            StructuralRefId   = e.StructuralRefId,
            EconomicRefType   = e.EconomicRefType,
            EconomicRefId     = e.EconomicRefId,
            IsRegistered      = true
        };
}
