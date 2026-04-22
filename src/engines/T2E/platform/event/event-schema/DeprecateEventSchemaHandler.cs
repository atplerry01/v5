using Whycespace.Domain.PlatformSystem.Event.EventSchema;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Event.EventSchema;

namespace Whycespace.Engines.T2E.Platform.Event.EventSchema;

public sealed class DeprecateEventSchemaHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DeprecateEventSchemaCommand cmd)
            return;

        var aggregate = (EventSchemaAggregate)await context.LoadAggregateAsync(typeof(EventSchemaAggregate));
        aggregate.Deprecate(new Timestamp(cmd.DeprecatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
