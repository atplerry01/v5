using Whycespace.Domain.ContentSystem.Media.Intake.Ingest;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Media.Intake.Ingest;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Media.Intake.Ingest;

public sealed class CompleteMediaIngestHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CompleteMediaIngestCommand cmd) return;
        var aggregate = (MediaIngestAggregate)await context.LoadAggregateAsync(typeof(MediaIngestAggregate));
        aggregate.Complete(new MediaIngestOutputRef(cmd.OutputRef), new Timestamp(cmd.CompletedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
