using Whycespace.Domain.ContentSystem.Document.Descriptor.Metadata;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.Descriptor.Metadata;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.Descriptor.Metadata;

public sealed class RemoveDocumentMetadataEntryHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RemoveDocumentMetadataEntryCommand cmd)
            return;

        var aggregate = (DocumentMetadataAggregate)await context.LoadAggregateAsync(typeof(DocumentMetadataAggregate));
        aggregate.RemoveEntry(
            new MetadataKey(cmd.Key),
            new Timestamp(cmd.RemovedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
