using Whycespace.Domain.ContentSystem.Document.Governance.Retention;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.Governance.Retention;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.Governance.Retention;

public sealed class PlaceRetentionHoldHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not PlaceRetentionHoldCommand cmd)
            return;

        var aggregate = (RetentionAggregate)await context.LoadAggregateAsync(typeof(RetentionAggregate));
        aggregate.PlaceHold(
            new RetentionReason(cmd.Reason),
            new Timestamp(cmd.PlacedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
