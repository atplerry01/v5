using Whycespace.Domain.PlatformSystem.Schema.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Schema.Serialization;

namespace Whycespace.Engines.T2E.Platform.Schema.Serialization;

public sealed class DeprecateSerializationFormatHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DeprecateSerializationFormatCommand cmd)
            return;

        var aggregate = (SerializationFormatAggregate)await context.LoadAggregateAsync(typeof(SerializationFormatAggregate));
        aggregate.Deprecate(new Timestamp(cmd.DeprecatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
