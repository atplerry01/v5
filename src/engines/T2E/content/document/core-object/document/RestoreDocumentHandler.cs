using Whycespace.Domain.ContentSystem.Document.CoreObject.Document;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Document;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.CoreObject.Document;

public sealed class RestoreDocumentHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RestoreDocumentCommand cmd)
            return;

        var aggregate = (DocumentAggregate)await context.LoadAggregateAsync(typeof(DocumentAggregate));
        aggregate.Restore(new Timestamp(cmd.RestoredAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
