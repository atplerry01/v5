using Whycespace.Domain.ContentSystem.Document.CoreObject.Template;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Template;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.CoreObject.Template;

public sealed class CreateDocumentTemplateHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateDocumentTemplateCommand cmd)
            return Task.CompletedTask;

        TemplateSchemaRef? schemaRef = cmd.SchemaRefId.HasValue
            ? new TemplateSchemaRef(cmd.SchemaRefId.Value)
            : null;

        var aggregate = DocumentTemplateAggregate.Create(
            new DocumentTemplateId(cmd.TemplateId),
            new TemplateName(cmd.Name),
            Enum.Parse<TemplateType>(cmd.Type),
            schemaRef,
            new Timestamp(cmd.CreatedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
