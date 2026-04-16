using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Learning.Course;

public sealed record DraftCourseCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid CorrelationId,
    Guid CausationId,
    Guid CourseId,
    string OwnerRef,
    string Title,
    DateTimeOffset OccurredAt) : IHasAggregateId
{
    public Guid AggregateId => CommandId;
}

public sealed record AttachCourseModuleCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid CorrelationId,
    Guid CausationId,
    string ModuleRef,
    int Order,
    DateTimeOffset OccurredAt) : IHasAggregateId
{
    public Guid AggregateId => CommandId;
}

public sealed record DetachCourseModuleCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid CorrelationId,
    Guid CausationId,
    string ModuleRef,
    DateTimeOffset OccurredAt) : IHasAggregateId
{
    public Guid AggregateId => CommandId;
}

public sealed record PublishCourseCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid CorrelationId,
    Guid CausationId,
    DateTimeOffset OccurredAt) : IHasAggregateId
{
    public Guid AggregateId => CommandId;
}

public sealed record ArchiveCourseCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid CorrelationId,
    Guid CausationId,
    DateTimeOffset OccurredAt) : IHasAggregateId
{
    public Guid AggregateId => CommandId;
}
