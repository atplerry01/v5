namespace Whycespace.Shared.Contracts.Events.Platform.Command.CommandRouting;

public sealed record CommandRoutingRegisteredEventSchema(
    Guid AggregateId,
    Guid CommandDefinitionId,
    string HandlerClassification,
    string HandlerContext,
    string HandlerDomain);

public sealed record CommandRoutingRemovedEventSchema(Guid AggregateId);
