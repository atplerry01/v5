using Whycespace.Domain.ContentSystem.Document.Intake.Upload;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.Intake.Upload;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.Intake.Upload;

public sealed class AcceptDocumentUploadHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not AcceptDocumentUploadCommand cmd)
            return;

        var aggregate = (DocumentUploadAggregate)await context.LoadAggregateAsync(typeof(DocumentUploadAggregate));
        aggregate.Accept(new Timestamp(cmd.AcceptedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
