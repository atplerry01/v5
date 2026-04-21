using Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Segment;
using Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.Segment;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Customer.SegmentationAndLifecycle.Segment;

public sealed class CreateSegmentHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateSegmentCommand cmd)
            return Task.CompletedTask;

        if (!Enum.TryParse<SegmentType>(cmd.Type, ignoreCase: true, out var type))
            throw new InvalidOperationException(
                $"Unknown SegmentType '{cmd.Type}'.");

        var aggregate = SegmentAggregate.Create(
            new SegmentId(cmd.SegmentId),
            new SegmentCode(cmd.Code),
            new SegmentName(cmd.Name),
            type,
            new SegmentCriteria(cmd.Criteria));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
