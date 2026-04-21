using Whycespace.Domain.ContentSystem.Document.CoreObject.Document;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Document;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.CoreObject.Document;

public sealed class UpdateDocumentMetadataHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not UpdateDocumentMetadataCommand cmd)
            return;

        var aggregate = (DocumentAggregate)await context.LoadAggregateAsync(typeof(DocumentAggregate));
        aggregate.UpdateMetadata(
            new DocumentTitle(cmd.NewTitle),
            Enum.Parse<DocumentType>(cmd.NewType),
            Enum.Parse<DocumentClassification>(cmd.NewClassification),
            new Timestamp(cmd.UpdatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
