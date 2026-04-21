using Whycespace.Domain.ContentSystem.Media.Intake.Ingest;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Media.Intake.Ingest;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Media.Intake.Ingest;

public sealed class CancelMediaIngestHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CancelMediaIngestCommand cmd) return;
        var aggregate = (MediaIngestAggregate)await context.LoadAggregateAsync(typeof(MediaIngestAggregate));
        aggregate.Cancel(new Timestamp(cmd.CancelledAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
