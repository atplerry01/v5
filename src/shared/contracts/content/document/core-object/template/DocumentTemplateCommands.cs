using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Document.CoreObject.Template;

public sealed record CreateDocumentTemplateCommand(
    Guid TemplateId,
    string Name,
    string Type,
    Guid? SchemaRefId,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => TemplateId;
}

public sealed record UpdateDocumentTemplateCommand(
    Guid TemplateId,
    string NewName,
    string NewType,
    Guid? NewSchemaRefId,
    DateTimeOffset UpdatedAt) : IHasAggregateId
{
    public Guid AggregateId => TemplateId;
}

public sealed record ActivateDocumentTemplateCommand(
    Guid TemplateId,
    DateTimeOffset ActivatedAt) : IHasAggregateId
{
    public Guid AggregateId => TemplateId;
}

public sealed record DeprecateDocumentTemplateCommand(
    Guid TemplateId,
    string Reason,
    DateTimeOffset DeprecatedAt) : IHasAggregateId
{
    public Guid AggregateId => TemplateId;
}

public sealed record ArchiveDocumentTemplateCommand(
    Guid TemplateId,
    DateTimeOffset ArchivedAt) : IHasAggregateId
{
    public Guid AggregateId => TemplateId;
}
