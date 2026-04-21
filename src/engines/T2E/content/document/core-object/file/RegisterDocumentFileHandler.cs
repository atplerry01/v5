using Whycespace.Domain.ContentSystem.Document.CoreObject.File;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.File;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.CoreObject.File;

public sealed class RegisterDocumentFileHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RegisterDocumentFileCommand cmd)
            return Task.CompletedTask;

        var aggregate = DocumentFileAggregate.Register(
            new DocumentFileId(cmd.DocumentFileId),
            new DocumentRef(cmd.DocumentId),
            new DocumentFileStorageRef(cmd.StorageRef),
            new DocumentFileChecksum(cmd.DeclaredChecksum),
            new DocumentFileMimeType(cmd.MimeType),
            new DocumentFileSize(cmd.SizeBytes),
            new Timestamp(cmd.RegisteredAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
