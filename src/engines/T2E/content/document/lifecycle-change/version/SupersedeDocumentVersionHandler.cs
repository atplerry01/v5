using Whycespace.Domain.ContentSystem.Document.LifecycleChange.Version;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.LifecycleChange.Version;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.LifecycleChange.Version;

public sealed class SupersedeDocumentVersionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not SupersedeDocumentVersionCommand cmd)
            return;

        var aggregate = (DocumentVersionAggregate)await context.LoadAggregateAsync(typeof(DocumentVersionAggregate));
        aggregate.Supersede(
            new DocumentVersionId(cmd.SuccessorVersionId),
            new Timestamp(cmd.SupersededAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
