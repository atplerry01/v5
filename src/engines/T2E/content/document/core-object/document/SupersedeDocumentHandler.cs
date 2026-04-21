using Whycespace.Domain.ContentSystem.Document.CoreObject.Document;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Document;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.CoreObject.Document;

public sealed class SupersedeDocumentHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not SupersedeDocumentCommand cmd)
            return;

        var aggregate = (DocumentAggregate)await context.LoadAggregateAsync(typeof(DocumentAggregate));
        aggregate.Supersede(new DocumentId(cmd.SupersedingDocumentId), new Timestamp(cmd.SupersededAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
