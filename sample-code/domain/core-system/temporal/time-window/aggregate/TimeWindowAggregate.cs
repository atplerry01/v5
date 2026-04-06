using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.Temporal.TimeWindow;

public sealed class TimeWindowAggregate : AggregateRoot
{
    public string WindowName { get; private set; } = string.Empty;
    public int DurationSeconds { get; private set; }
    public bool IsClosed { get; private set; }

    public static TimeWindowAggregate Create(Guid id, string windowName, int durationSeconds)
    {
        var agg = new TimeWindowAggregate
        {
            Id = id,
            WindowName = windowName,
            DurationSeconds = durationSeconds,
            IsClosed = false
        };
        agg.RaiseDomainEvent(new TimeWindowCreatedEvent(id, windowName, durationSeconds));
        return agg;
    }

    public void Close()
    {
        IsClosed = true;
        RaiseDomainEvent(new TimeWindowClosedEvent(Id));
    }
}
