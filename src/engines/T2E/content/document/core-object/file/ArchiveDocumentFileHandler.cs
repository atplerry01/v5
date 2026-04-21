using Whycespace.Domain.ContentSystem.Document.CoreObject.File;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.File;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.CoreObject.File;

public sealed class ArchiveDocumentFileHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ArchiveDocumentFileCommand cmd)
            return;

        var aggregate = (DocumentFileAggregate)await context.LoadAggregateAsync(typeof(DocumentFileAggregate));
        aggregate.Archive(new Timestamp(cmd.ArchivedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
