using Whycespace.Domain.ContentSystem.Document.CoreObject.Document;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Document;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.CoreObject.Document;

public sealed class CreateDocumentHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateDocumentCommand cmd)
            return Task.CompletedTask;

        var aggregate = DocumentAggregate.Create(
            new DocumentId(cmd.DocumentId),
            new DocumentTitle(cmd.Title),
            Enum.Parse<DocumentType>(cmd.Type),
            Enum.Parse<DocumentClassification>(cmd.Classification),
            new StructuralOwnerRef(cmd.StructuralOwnerId),
            new BusinessOwnerRef(
                Enum.Parse<BusinessOwnerKind>(cmd.BusinessOwnerKind),
                cmd.BusinessOwnerId),
            new Timestamp(cmd.CreatedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
