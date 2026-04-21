using Whycespace.Domain.ContentSystem.Document.LifecycleChange.Processing;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.LifecycleChange.Processing;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.LifecycleChange.Processing;

public sealed class StartDocumentProcessingHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not StartDocumentProcessingCommand cmd)
            return;

        var aggregate = (DocumentProcessingAggregate)await context.LoadAggregateAsync(typeof(DocumentProcessingAggregate));
        aggregate.Start(new Timestamp(cmd.StartedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
