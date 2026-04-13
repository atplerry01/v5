namespace Whycespace.Shared.Contracts.Engine;

public interface IEngineContext
{
    object Command { get; }
    Guid AggregateId { get; }
    Task<object> LoadAggregateAsync(Type aggregateType);
    void EmitEvents(IReadOnlyList<object> events);
    IReadOnlyList<object> EmittedEvents { get; }
}
