namespace Whycespace.Shared.Contracts.Engine;

/// <summary>
/// Marker interface for aggregates that produce domain events.
/// Implemented by AggregateRoot in the domain layer — this contract lives
/// in shared so EngineContext can collect events without referencing domain types.
/// </summary>
public interface IEventSource
{
    IReadOnlyList<object> GetPendingEvents();
    void ClearPendingEvents();
}
