namespace Whycespace.Shared.Contracts.Events.Structural.Structure.HierarchyDefinition;

public sealed record HierarchyDefinitionDefinedEventSchema(
    Guid AggregateId,
    string HierarchyName,
    Guid ParentReference);

public sealed record HierarchyDefinitionValidatedEventSchema(
    Guid AggregateId);

public sealed record HierarchyDefinitionLockedEventSchema(
    Guid AggregateId);
