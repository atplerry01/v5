using Whycespace.Domain.PlatformSystem.Command.CommandRouting;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Command.CommandRouting;

namespace Whycespace.Engines.T2E.Platform.Command.CommandRouting;

public sealed class RegisterCommandRoutingRuleHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RegisterCommandRoutingRuleCommand cmd)
            return Task.CompletedTask;

        var aggregate = CommandRoutingRuleAggregate.Register(
            new CommandRoutingRuleId(cmd.CommandRoutingRuleId),
            new CommandTypeRef(cmd.CommandDefinitionId),
            new DomainRoute(cmd.HandlerClassification, cmd.HandlerContext, cmd.HandlerDomain),
            new Timestamp(cmd.RegisteredAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
