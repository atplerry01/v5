using Whycespace.Shared.Contracts.Events.Structural.Structure.Classification;
using Whycespace.Shared.Contracts.Structural.Structure.Classification;

namespace Whycespace.Projections.Structural.Structure.Classification.Reducer;

public static class ClassificationProjectionReducer
{
    public static ClassificationReadModel Apply(ClassificationReadModel state, ClassificationDefinedEventSchema e, DateTimeOffset at) =>
        state with
        {
            ClassificationId = e.AggregateId,
            ClassificationName = e.ClassificationName,
            ClassificationCategory = e.ClassificationCategory,
            Status = "Defined",
            LastModifiedAt = at
        };

    public static ClassificationReadModel Apply(ClassificationReadModel state, ClassificationActivatedEventSchema e, DateTimeOffset at) =>
        state with { ClassificationId = e.AggregateId, Status = "Active", LastModifiedAt = at };

    public static ClassificationReadModel Apply(ClassificationReadModel state, ClassificationDeprecatedEventSchema e, DateTimeOffset at) =>
        state with { ClassificationId = e.AggregateId, Status = "Deprecated", LastModifiedAt = at };
}
