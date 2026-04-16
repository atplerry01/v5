using Whycespace.Domain.ContentSystem.Learning.Course;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Learning.Course;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Learning.Course;

public sealed class AttachCourseModuleHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not AttachCourseModuleCommand cmd)
            return;

        var aggregate = (CourseAggregate)await context.LoadAggregateAsync(typeof(CourseAggregate));
        aggregate.AttachModule(
            new EventId(cmd.CommandId),
            new AggregateId(cmd.AggregateId),
            new CorrelationId(cmd.CorrelationId),
            new CausationId(cmd.CausationId),
            cmd.ModuleRef,
            cmd.Order,
            new Timestamp(cmd.OccurredAt));

        context.EmitEvents(aggregate.DomainEvents);
    }
}
