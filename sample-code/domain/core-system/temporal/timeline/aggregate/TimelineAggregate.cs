using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.Temporal.Timeline;

public sealed class TimelineAggregate : AggregateRoot
{
    public string TimelineName { get; private set; } = string.Empty;
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset? EndDate { get; private set; }

    public static TimelineAggregate Create(Guid id, string timelineName, DateTimeOffset startDate)
    {
        var agg = new TimelineAggregate
        {
            Id = id,
            TimelineName = timelineName,
            StartDate = startDate
        };
        agg.RaiseDomainEvent(new TimelineCreatedEvent(id, timelineName, startDate));
        return agg;
    }

    public void Extend(DateTimeOffset newEndDate)
    {
        EndDate = newEndDate;
        RaiseDomainEvent(new TimelineExtendedEvent(Id, newEndDate));
    }
}
