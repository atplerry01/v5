using Whycespace.Domain.ContentSystem.Document.Intake.Upload;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.Intake.Upload;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.Intake.Upload;

public sealed class RequestDocumentUploadHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RequestDocumentUploadCommand cmd)
            return Task.CompletedTask;

        var aggregate = DocumentUploadAggregate.Request(
            new DocumentUploadId(cmd.UploadId),
            new DocumentUploadSourceRef(cmd.SourceRef),
            new DocumentUploadInputRef(cmd.InputRef),
            new Timestamp(cmd.RequestedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
