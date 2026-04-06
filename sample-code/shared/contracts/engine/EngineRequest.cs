namespace Whycespace.Shared.Contracts.Engine;

/// <summary>
/// Request object passed from the runtime to an engine.
/// Contains the command type, serialized payload, and correlation metadata.
/// </summary>
public sealed record EngineRequest
{
    public required string CommandType { get; init; }
    public required object Payload { get; init; }
    public required string CorrelationId { get; init; }
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Creates an EngineContext from this request for use by typed engines.
    /// </summary>
    public EngineContext ToContext() => new()
    {
        CommandType = CommandType,
        Payload = Payload,
        CorrelationId = CorrelationId,
        Headers = Headers
    };

    /// <summary>
    /// Creates an EngineContext with an injected aggregate store.
    /// </summary>
    public EngineContext ToContext(IAggregateStore aggregateStore) => new(aggregateStore)
    {
        CommandType = CommandType,
        Payload = Payload,
        CorrelationId = CorrelationId,
        Headers = Headers
    };
}
