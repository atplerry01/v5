using Whycespace.Domain.ContentSystem.Media.Intake.Ingest;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Media.Intake.Ingest;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Media.Intake.Ingest;

public sealed class FailMediaIngestHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not FailMediaIngestCommand cmd) return;
        var aggregate = (MediaIngestAggregate)await context.LoadAggregateAsync(typeof(MediaIngestAggregate));
        aggregate.Fail(new MediaIngestFailureReason(cmd.Reason), new Timestamp(cmd.FailedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
