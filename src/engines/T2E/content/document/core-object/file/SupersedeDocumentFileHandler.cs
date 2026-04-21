using Whycespace.Domain.ContentSystem.Document.CoreObject.File;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.File;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.CoreObject.File;

public sealed class SupersedeDocumentFileHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not SupersedeDocumentFileCommand cmd)
            return;

        var aggregate = (DocumentFileAggregate)await context.LoadAggregateAsync(typeof(DocumentFileAggregate));
        aggregate.Supersede(
            new DocumentFileId(cmd.SuccessorFileId),
            new Timestamp(cmd.SupersededAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
