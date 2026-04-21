using Whycespace.Domain.ContentSystem.Document.Descriptor.Metadata;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.Descriptor.Metadata;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.Descriptor.Metadata;

public sealed class CreateDocumentMetadataHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateDocumentMetadataCommand cmd)
            return Task.CompletedTask;

        var aggregate = DocumentMetadataAggregate.Create(
            new DocumentMetadataId(cmd.MetadataId),
            new DocumentRef(cmd.DocumentId),
            new Timestamp(cmd.CreatedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
