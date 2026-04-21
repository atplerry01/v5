namespace Whycespace.Shared.Contracts.Events.Structural.Structure.TypeDefinition;

public sealed record TypeDefinitionDefinedEventSchema(
    Guid AggregateId,
    string TypeName,
    string TypeCategory);

public sealed record TypeDefinitionActivatedEventSchema(
    Guid AggregateId);

public sealed record TypeDefinitionRetiredEventSchema(
    Guid AggregateId);
