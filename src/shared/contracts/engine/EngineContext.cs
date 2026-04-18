namespace Whycespace.Shared.Contracts.Engine;

public sealed class EngineContext : IEngineContext
{
    public object Command { get; }
    public Guid AggregateId { get; }
    public string? EnforcementConstraint { get; }
    public bool IsSystem { get; }
    private readonly Func<Type, Guid, Task<object>> _aggregateLoader;
    private readonly List<object> _emittedEvents = new();

    public IReadOnlyList<object> EmittedEvents => _emittedEvents.AsReadOnly();

    public EngineContext(
        object command,
        Guid aggregateId,
        Func<Type, Guid, Task<object>> aggregateLoader)
        : this(command, aggregateId, aggregateLoader, enforcementConstraint: null, isSystem: false) { }

    public EngineContext(
        object command,
        Guid aggregateId,
        Func<Type, Guid, Task<object>> aggregateLoader,
        string? enforcementConstraint)
        : this(command, aggregateId, aggregateLoader, enforcementConstraint, isSystem: false) { }

    public EngineContext(
        object command,
        Guid aggregateId,
        Func<Type, Guid, Task<object>> aggregateLoader,
        string? enforcementConstraint,
        bool isSystem)
    {
        Command = command;
        AggregateId = aggregateId;
        _aggregateLoader = aggregateLoader;
        EnforcementConstraint = enforcementConstraint;
        IsSystem = isSystem;
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
