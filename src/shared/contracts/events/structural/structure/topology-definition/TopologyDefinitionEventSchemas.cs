namespace Whycespace.Shared.Contracts.Events.Structural.Structure.TopologyDefinition;

public sealed record TopologyDefinitionCreatedEventSchema(
    Guid AggregateId,
    string DefinitionName,
    string DefinitionKind);

public sealed record TopologyDefinitionActivatedEventSchema(
    Guid AggregateId);

public sealed record TopologyDefinitionSuspendedEventSchema(
    Guid AggregateId);

public sealed record TopologyDefinitionReactivatedEventSchema(
    Guid AggregateId);

public sealed record TopologyDefinitionRetiredEventSchema(
    Guid AggregateId);
