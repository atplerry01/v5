using Whycespace.Domain.ContentSystem.Document.CoreObject.Document;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Document;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.CoreObject.Document;

public sealed class ActivateDocumentHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateDocumentCommand cmd)
            return;

        var aggregate = (DocumentAggregate)await context.LoadAggregateAsync(typeof(DocumentAggregate));
        aggregate.Activate(new Timestamp(cmd.ActivatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
