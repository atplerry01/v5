using Whycespace.Domain.ContentSystem.Document.CoreObject.Template;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Template;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.CoreObject.Template;

public sealed class ActivateDocumentTemplateHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateDocumentTemplateCommand cmd)
            return;

        var aggregate = (DocumentTemplateAggregate)await context.LoadAggregateAsync(typeof(DocumentTemplateAggregate));
        aggregate.Activate(new Timestamp(cmd.ActivatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
