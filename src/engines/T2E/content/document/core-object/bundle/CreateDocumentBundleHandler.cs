using Whycespace.Domain.ContentSystem.Document.CoreObject.Bundle;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Bundle;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.CoreObject.Bundle;

public sealed class CreateDocumentBundleHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateDocumentBundleCommand cmd)
            return Task.CompletedTask;

        var aggregate = DocumentBundleAggregate.Create(
            new DocumentBundleId(cmd.BundleId),
            new BundleName(cmd.Name),
            new Timestamp(cmd.CreatedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
