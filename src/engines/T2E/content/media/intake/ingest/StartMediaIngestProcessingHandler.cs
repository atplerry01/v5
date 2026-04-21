using Whycespace.Domain.ContentSystem.Media.Intake.Ingest;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Media.Intake.Ingest;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Media.Intake.Ingest;

public sealed class StartMediaIngestProcessingHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not StartMediaIngestProcessingCommand cmd) return;
        var aggregate = (MediaIngestAggregate)await context.LoadAggregateAsync(typeof(MediaIngestAggregate));
        aggregate.StartProcessing(new Timestamp(cmd.StartedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
