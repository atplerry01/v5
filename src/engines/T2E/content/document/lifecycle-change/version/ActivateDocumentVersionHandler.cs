using Whycespace.Domain.ContentSystem.Document.LifecycleChange.Version;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.LifecycleChange.Version;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.LifecycleChange.Version;

public sealed class ActivateDocumentVersionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateDocumentVersionCommand cmd)
            return;

        var aggregate = (DocumentVersionAggregate)await context.LoadAggregateAsync(typeof(DocumentVersionAggregate));
        aggregate.Activate(new Timestamp(cmd.ActivatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
