using Whycespace.Domain.ContentSystem.Learning.Course;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Learning.Course;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Learning.Course;

public sealed class DraftCourseHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DraftCourseCommand cmd)
            return Task.CompletedTask;

        var aggregate = CourseAggregate.Draft(
            new EventId(cmd.CommandId),
            new AggregateId(cmd.AggregateId),
            new CorrelationId(cmd.CorrelationId),
            new CausationId(cmd.CausationId),
            CourseId.From(cmd.CourseId),
            cmd.OwnerRef,
            CourseTitle.Create(cmd.Title),
            new Timestamp(cmd.OccurredAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
