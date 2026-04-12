namespace Whycespace.Domain.CoreSystem.Command.CommandRouting;

public sealed record CommandRoutingDefinedEvent(
    CommandRoutingId RoutingId,
    RoutingRule Rule);
