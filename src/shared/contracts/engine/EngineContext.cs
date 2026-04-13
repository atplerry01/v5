namespace Whycespace.Shared.Contracts.Engine;

public sealed class EngineContext : IEngineContext
{
    public object Command { get; }
    public Guid AggregateId { get; }
    private readonly Func<Type, Guid, Task<object>> _aggregateLoader;
    private readonly List<object> _emittedEvents = new();

    public IReadOnlyList<object> EmittedEvents => _emittedEvents.AsReadOnly();

    public EngineContext(
        object command,
        Guid aggregateId,
        Func<Type, Guid, Task<object>> aggregateLoader)
    {
        Command = command;
        AggregateId = aggregateId;
        _aggregateLoader = aggregateLoader;
    }

    public async Task<object> LoadAggregateAsync(Type aggregateType)
    {
        return await _aggregateLoader(aggregateType, AggregateId);
    }

    public void EmitEvents(IReadOnlyList<object> events)
    {
        _emittedEvents.AddRange(events);
    }
}
