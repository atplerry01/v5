using Whycespace.Domain.PlatformSystem.Event.EventDefinition;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Event.EventDefinition;

namespace Whycespace.Engines.T2E.Platform.Event.EventDefinition;

public sealed class DeprecateEventDefinitionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DeprecateEventDefinitionCommand cmd)
            return;

        var aggregate = (EventDefinitionAggregate)await context.LoadAggregateAsync(typeof(EventDefinitionAggregate));
        aggregate.Deprecate(new Timestamp(cmd.DeprecatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
