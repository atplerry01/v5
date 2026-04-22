using Whycespace.Domain.PlatformSystem.Envelope.Payload;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Envelope.Payload;

namespace Whycespace.Engines.T2E.Platform.Envelope.Payload;

public sealed class DeprecatePayloadSchemaHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DeprecatePayloadSchemaCommand cmd)
            return;

        var aggregate = (PayloadSchemaAggregate)await context.LoadAggregateAsync(typeof(PayloadSchemaAggregate));
        aggregate.Deprecate(new Timestamp(cmd.DeprecatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
