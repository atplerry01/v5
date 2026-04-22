using Whycespace.Domain.PlatformSystem.Command.CommandDefinition;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Command.CommandDefinition;

namespace Whycespace.Engines.T2E.Platform.Command.CommandDefinition;

public sealed class DeprecateCommandDefinitionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DeprecateCommandDefinitionCommand cmd)
            return;

        var aggregate = (CommandDefinitionAggregate)await context.LoadAggregateAsync(typeof(CommandDefinitionAggregate));
        aggregate.Deprecate(new Timestamp(cmd.DeprecatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
