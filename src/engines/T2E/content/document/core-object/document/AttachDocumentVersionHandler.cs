using Whycespace.Domain.ContentSystem.Document.CoreObject.Document;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Document;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.CoreObject.Document;

public sealed class AttachDocumentVersionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not AttachDocumentVersionCommand cmd)
            return;

        var aggregate = (DocumentAggregate)await context.LoadAggregateAsync(typeof(DocumentAggregate));
        aggregate.AttachVersion(new CurrentVersionRef(cmd.VersionId), new Timestamp(cmd.AttachedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
