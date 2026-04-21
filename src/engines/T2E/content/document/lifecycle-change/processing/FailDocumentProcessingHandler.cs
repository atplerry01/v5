using Whycespace.Domain.ContentSystem.Document.LifecycleChange.Processing;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.LifecycleChange.Processing;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.LifecycleChange.Processing;

public sealed class FailDocumentProcessingHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not FailDocumentProcessingCommand cmd)
            return;

        var aggregate = (DocumentProcessingAggregate)await context.LoadAggregateAsync(typeof(DocumentProcessingAggregate));
        aggregate.Fail(
            new ProcessingFailureReason(cmd.Reason),
            new Timestamp(cmd.FailedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
