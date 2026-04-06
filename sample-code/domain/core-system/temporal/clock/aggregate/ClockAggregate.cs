using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.Temporal.Clock;

public sealed class ClockAggregate : AggregateRoot
{
    public string ClockName { get; private set; } = string.Empty;
    public string Timezone { get; private set; } = string.Empty;
    public bool IsPaused { get; private set; }

    public static ClockAggregate Create(Guid id, string clockName, string timezone)
    {
        var agg = new ClockAggregate
        {
            Id = id,
            ClockName = clockName,
            Timezone = timezone,
            IsPaused = false
        };
        agg.RaiseDomainEvent(new ClockCreatedEvent(id, clockName, timezone));
        return agg;
    }

    public void Pause()
    {
        IsPaused = true;
        RaiseDomainEvent(new ClockPausedEvent(Id));
    }
}
