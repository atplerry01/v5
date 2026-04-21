using Whycespace.Domain.ContentSystem.Document.LifecycleChange.Version;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.LifecycleChange.Version;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.LifecycleChange.Version;

public sealed class CreateDocumentVersionHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateDocumentVersionCommand cmd)
            return Task.CompletedTask;

        DocumentVersionId? previousVersionId = cmd.PreviousVersionId.HasValue
            ? new DocumentVersionId(cmd.PreviousVersionId.Value)
            : null;

        var aggregate = DocumentVersionAggregate.Create(
            new DocumentVersionId(cmd.VersionId),
            new DocumentRef(cmd.DocumentRef),
            new VersionNumber(cmd.Major, cmd.Minor),
            new ArtifactRef(cmd.ArtifactRef),
            previousVersionId,
            new Timestamp(cmd.CreatedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
