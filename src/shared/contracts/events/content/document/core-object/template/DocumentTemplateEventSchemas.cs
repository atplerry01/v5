namespace Whycespace.Shared.Contracts.Events.Content.Document.CoreObject.Template;

public sealed record DocumentTemplateCreatedEventSchema(
    Guid AggregateId,
    string Name,
    string Type,
    Guid? SchemaRefId,
    DateTimeOffset CreatedAt);

public sealed record DocumentTemplateUpdatedEventSchema(
    Guid AggregateId,
    string PreviousName,
    string NewName,
    string PreviousType,
    string NewType,
    Guid? NewSchemaRefId,
    DateTimeOffset UpdatedAt);

public sealed record DocumentTemplateActivatedEventSchema(
    Guid AggregateId,
    DateTimeOffset ActivatedAt);

public sealed record DocumentTemplateDeprecatedEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset DeprecatedAt);

public sealed record DocumentTemplateArchivedEventSchema(
    Guid AggregateId,
    DateTimeOffset ArchivedAt);
