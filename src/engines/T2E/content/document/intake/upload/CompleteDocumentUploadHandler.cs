using Whycespace.Domain.ContentSystem.Document.Intake.Upload;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.Intake.Upload;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.Intake.Upload;

public sealed class CompleteDocumentUploadHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CompleteDocumentUploadCommand cmd)
            return;

        var aggregate = (DocumentUploadAggregate)await context.LoadAggregateAsync(typeof(DocumentUploadAggregate));
        aggregate.Complete(
            new DocumentUploadOutputRef(cmd.OutputRef),
            new Timestamp(cmd.CompletedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
