using Whycespace.Domain.ContentSystem.Document.CoreObject.Record;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Record;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.CoreObject.Record;

public sealed class ArchiveDocumentRecordHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ArchiveDocumentRecordCommand cmd)
            return;

        var aggregate = (DocumentRecordAggregate)await context.LoadAggregateAsync(typeof(DocumentRecordAggregate));
        aggregate.Archive(new Timestamp(cmd.ArchivedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
