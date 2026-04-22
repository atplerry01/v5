using Whycespace.Domain.PlatformSystem.Envelope.Header;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Envelope.Header;

namespace Whycespace.Engines.T2E.Platform.Envelope.Header;

public sealed class DeprecateHeaderSchemaHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DeprecateHeaderSchemaCommand cmd)
            return;

        var aggregate = (HeaderSchemaAggregate)await context.LoadAggregateAsync(typeof(HeaderSchemaAggregate));
        aggregate.Deprecate(new Timestamp(cmd.DeprecatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
