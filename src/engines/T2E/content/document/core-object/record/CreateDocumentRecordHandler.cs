using Whycespace.Domain.ContentSystem.Document.CoreObject.Record;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Record;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.CoreObject.Record;

public sealed class CreateDocumentRecordHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateDocumentRecordCommand cmd)
            return Task.CompletedTask;

        var aggregate = DocumentRecordAggregate.Create(
            new DocumentRecordId(cmd.RecordId),
            new DocumentRef(cmd.DocumentId),
            new Timestamp(cmd.CreatedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
