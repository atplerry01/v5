namespace Whycespace.Shared.Contracts.Events.Content.Learning.Course;

public sealed record CourseDraftedEventSchema(
    Guid AggregateId,
    Guid CourseId,
    string OwnerRef,
    string Title,
    DateTimeOffset DraftedAt);

public sealed record CourseModuleAttachedEventSchema(
    Guid AggregateId,
    Guid CourseId,
    string ModuleRef,
    int Order,
    DateTimeOffset AttachedAt);

public sealed record CourseModuleDetachedEventSchema(
    Guid AggregateId,
    Guid CourseId,
    string ModuleRef,
    DateTimeOffset DetachedAt);

public sealed record CoursePublishedEventSchema(
    Guid AggregateId,
    Guid CourseId,
    DateTimeOffset PublishedAt);

public sealed record CourseArchivedEventSchema(
    Guid AggregateId,
    Guid CourseId,
    DateTimeOffset ArchivedAt);
