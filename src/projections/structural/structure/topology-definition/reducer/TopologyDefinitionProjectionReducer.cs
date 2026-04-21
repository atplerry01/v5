using Whycespace.Shared.Contracts.Events.Structural.Structure.TopologyDefinition;
using Whycespace.Shared.Contracts.Structural.Structure.TopologyDefinition;

namespace Whycespace.Projections.Structural.Structure.TopologyDefinition.Reducer;

public static class TopologyDefinitionProjectionReducer
{
    public static TopologyDefinitionReadModel Apply(TopologyDefinitionReadModel state, TopologyDefinitionCreatedEventSchema e, DateTimeOffset at) =>
        state with
        {
            TopologyDefinitionId = e.AggregateId,
            DefinitionName = e.DefinitionName,
            DefinitionKind = e.DefinitionKind,
            Status = "Draft",
            LastModifiedAt = at
        };

    public static TopologyDefinitionReadModel Apply(TopologyDefinitionReadModel state, TopologyDefinitionActivatedEventSchema e, DateTimeOffset at) =>
        state with { TopologyDefinitionId = e.AggregateId, Status = "Active", LastModifiedAt = at };

    public static TopologyDefinitionReadModel Apply(TopologyDefinitionReadModel state, TopologyDefinitionSuspendedEventSchema e, DateTimeOffset at) =>
        state with { TopologyDefinitionId = e.AggregateId, Status = "Suspended", LastModifiedAt = at };

    public static TopologyDefinitionReadModel Apply(TopologyDefinitionReadModel state, TopologyDefinitionReactivatedEventSchema e, DateTimeOffset at) =>
        state with { TopologyDefinitionId = e.AggregateId, Status = "Active", LastModifiedAt = at };

    public static TopologyDefinitionReadModel Apply(TopologyDefinitionReadModel state, TopologyDefinitionRetiredEventSchema e, DateTimeOffset at) =>
        state with { TopologyDefinitionId = e.AggregateId, Status = "Retired", LastModifiedAt = at };
}
