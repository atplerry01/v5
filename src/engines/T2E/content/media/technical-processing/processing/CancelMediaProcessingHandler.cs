using Whycespace.Domain.ContentSystem.Media.TechnicalProcessing.Processing;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Media.TechnicalProcessing.Processing;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Media.TechnicalProcessing.Processing;

public sealed class CancelMediaProcessingHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CancelMediaProcessingCommand cmd) return;
        var aggregate = (MediaProcessingAggregate)await context.LoadAggregateAsync(typeof(MediaProcessingAggregate));
        aggregate.Cancel(new Timestamp(cmd.CancelledAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
