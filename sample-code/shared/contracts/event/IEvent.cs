namespace Whycespace.Shared.Contracts.Event;

public interface IEvent
{
    string EventId { get; }
    string EventType { get; }
    DateTime OccurredAt { get; }
}
