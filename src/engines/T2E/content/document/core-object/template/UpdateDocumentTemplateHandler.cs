using Whycespace.Domain.ContentSystem.Document.CoreObject.Template;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Template;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.CoreObject.Template;

public sealed class UpdateDocumentTemplateHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not UpdateDocumentTemplateCommand cmd)
            return;

        var aggregate = (DocumentTemplateAggregate)await context.LoadAggregateAsync(typeof(DocumentTemplateAggregate));

        TemplateSchemaRef? schemaRef = cmd.NewSchemaRefId.HasValue
            ? new TemplateSchemaRef(cmd.NewSchemaRefId.Value)
            : null;

        aggregate.Update(
            new TemplateName(cmd.NewName),
            Enum.Parse<TemplateType>(cmd.NewType),
            schemaRef,
            new Timestamp(cmd.UpdatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
