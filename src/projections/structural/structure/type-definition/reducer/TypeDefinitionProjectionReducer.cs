using Whycespace.Shared.Contracts.Events.Structural.Structure.TypeDefinition;
using Whycespace.Shared.Contracts.Structural.Structure.TypeDefinition;

namespace Whycespace.Projections.Structural.Structure.TypeDefinition.Reducer;

public static class TypeDefinitionProjectionReducer
{
    public static TypeDefinitionReadModel Apply(TypeDefinitionReadModel state, TypeDefinitionDefinedEventSchema e, DateTimeOffset at) =>
        state with
        {
            TypeDefinitionId = e.AggregateId,
            TypeName = e.TypeName,
            TypeCategory = e.TypeCategory,
            Status = "Defined",
            LastModifiedAt = at
        };

    public static TypeDefinitionReadModel Apply(TypeDefinitionReadModel state, TypeDefinitionActivatedEventSchema e, DateTimeOffset at) =>
        state with { TypeDefinitionId = e.AggregateId, Status = "Active", LastModifiedAt = at };

    public static TypeDefinitionReadModel Apply(TypeDefinitionReadModel state, TypeDefinitionRetiredEventSchema e, DateTimeOffset at) =>
        state with { TypeDefinitionId = e.AggregateId, Status = "Retired", LastModifiedAt = at };
}
