using Whycespace.Domain.ContentSystem.Document.Governance.Retention;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.Governance.Retention;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.Governance.Retention;

public sealed class ReleaseRetentionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ReleaseRetentionCommand cmd)
            return;

        var aggregate = (RetentionAggregate)await context.LoadAggregateAsync(typeof(RetentionAggregate));
        aggregate.Release(new Timestamp(cmd.ReleasedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
