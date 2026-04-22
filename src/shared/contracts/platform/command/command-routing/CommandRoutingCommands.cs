using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Platform.Command.CommandRouting;

public sealed record RegisterCommandRoutingRuleCommand(
    Guid CommandRoutingRuleId,
    Guid CommandDefinitionId,
    string HandlerClassification,
    string HandlerContext,
    string HandlerDomain,
    DateTimeOffset RegisteredAt) : IHasAggregateId
{
    public Guid AggregateId => CommandRoutingRuleId;
}

public sealed record RemoveCommandRoutingRuleCommand(
    Guid CommandRoutingRuleId,
    DateTimeOffset RemovedAt) : IHasAggregateId
{
    public Guid AggregateId => CommandRoutingRuleId;
}
