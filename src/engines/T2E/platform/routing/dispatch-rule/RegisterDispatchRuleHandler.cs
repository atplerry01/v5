using Whycespace.Domain.PlatformSystem.Routing.DispatchRule;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Routing.DispatchRule;

namespace Whycespace.Engines.T2E.Platform.Routing.DispatchRule;

public sealed class RegisterDispatchRuleHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RegisterDispatchRuleCommand cmd)
            return Task.CompletedTask;

        var conditionType = cmd.ConditionType switch
        {
            "MessageKindMatch" => DispatchConditionType.MessageKindMatch,
            "SourceClassificationMatch" => DispatchConditionType.SourceClassificationMatch,
            "DestinationContextMatch" => DispatchConditionType.DestinationContextMatch,
            "TransportHintMatch" => DispatchConditionType.TransportHintMatch,
            _ => DispatchConditionType.AlwaysMatch
        };

        var aggregate = DispatchRuleAggregate.Register(
            new DispatchRuleId(cmd.DispatchRuleId),
            cmd.RuleName,
            cmd.RouteRef,
            new DispatchCondition(conditionType, cmd.MatchValue),
            cmd.Priority,
            new Timestamp(cmd.RegisteredAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
