using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.Temporal.Clock;

public sealed record ClockCreatedEvent(Guid ClockId, string ClockName, string Timezone) : DomainEvent;
public sealed record ClockPausedEvent(Guid ClockId) : DomainEvent;
