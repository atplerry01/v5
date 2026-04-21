using Whycespace.Shared.Contracts.Events.Structural.Structure.HierarchyDefinition;
using Whycespace.Shared.Contracts.Structural.Structure.HierarchyDefinition;

namespace Whycespace.Projections.Structural.Structure.HierarchyDefinition.Reducer;

public static class HierarchyDefinitionProjectionReducer
{
    public static HierarchyDefinitionReadModel Apply(HierarchyDefinitionReadModel state, HierarchyDefinitionDefinedEventSchema e, DateTimeOffset at) =>
        state with
        {
            HierarchyDefinitionId = e.AggregateId,
            HierarchyName = e.HierarchyName,
            ParentReference = e.ParentReference,
            Status = "Defined",
            LastModifiedAt = at
        };

    public static HierarchyDefinitionReadModel Apply(HierarchyDefinitionReadModel state, HierarchyDefinitionValidatedEventSchema e, DateTimeOffset at) =>
        state with { HierarchyDefinitionId = e.AggregateId, Status = "Validated", LastModifiedAt = at };

    public static HierarchyDefinitionReadModel Apply(HierarchyDefinitionReadModel state, HierarchyDefinitionLockedEventSchema e, DateTimeOffset at) =>
        state with { HierarchyDefinitionId = e.AggregateId, Status = "Locked", LastModifiedAt = at };
}
