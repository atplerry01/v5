using Whycespace.Domain.ContentSystem.Document.Intake.Upload;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.Intake.Upload;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.Intake.Upload;

public sealed class FailDocumentUploadHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not FailDocumentUploadCommand cmd)
            return;

        var aggregate = (DocumentUploadAggregate)await context.LoadAggregateAsync(typeof(DocumentUploadAggregate));
        aggregate.Fail(
            new DocumentUploadFailureReason(cmd.Reason),
            new Timestamp(cmd.FailedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
