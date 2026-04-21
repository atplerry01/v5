using Whycespace.Domain.ContentSystem.Media.TechnicalProcessing.Processing;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Media.TechnicalProcessing.Processing;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Media.TechnicalProcessing.Processing;

public sealed class StartMediaProcessingHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not StartMediaProcessingCommand cmd) return;
        var aggregate = (MediaProcessingAggregate)await context.LoadAggregateAsync(typeof(MediaProcessingAggregate));
        aggregate.Start(new Timestamp(cmd.StartedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
