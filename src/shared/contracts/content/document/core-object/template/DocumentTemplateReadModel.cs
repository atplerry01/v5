namespace Whycespace.Shared.Contracts.Content.Document.CoreObject.Template;

public sealed record DocumentTemplateReadModel
{
    public Guid TemplateId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public Guid? SchemaRefId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
