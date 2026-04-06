using System.Text.Json;
using Whycespace.Shared.Contracts.Engine;
using IAggregateStore = Whycespace.Shared.Contracts.Engine.IAggregateStore;

namespace Whycespace.Runtime.Engine;

/// <summary>
/// Marker interface for adapter detection.
/// Used by EngineResolver to enforce adapter-only registration.
/// </summary>
public interface ITypedEngineAdapter
{
    Type CommandType { get; }
}

/// <summary>
/// Bridges a typed IEngine&lt;TCommand&gt; to the runtime's untyped IEngine contract.
/// Handles deserialization of the payload to the strongly-typed command and delegates
/// to the typed engine. This eliminates the need for manual command handler facades
/// while keeping engines type-safe internally.
///
/// Usage:
///   builder.Engines.Register("todo.create",
///       new EngineDescriptor {
///           Name = "todo.create", Version = "v1",
///           CommandType = typeof(CreateTodoCommand),
///           Engine = new TypedEngineAdapter&lt;CreateTodoCommand&gt;("todo-create", new TodoCreateEngine())
///       });
/// </summary>
public sealed class TypedEngineAdapter<TCommand> : IEngine, ITypedEngineAdapter where TCommand : notnull
{
    private readonly string _engineId;
    private readonly IEngine<TCommand> _inner;
    private readonly IAggregateStore? _aggregateStore;

    public TypedEngineAdapter(string engineId, IEngine<TCommand> inner, IAggregateStore? aggregateStore = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(engineId);
        ArgumentNullException.ThrowIfNull(inner);
        _engineId = engineId;
        _inner = inner;
        _aggregateStore = aggregateStore;
    }

    public string EngineId => _engineId;
    public Type CommandType => typeof(TCommand);

    public async Task<EngineResult> ExecuteAsync(EngineRequest request, CancellationToken cancellationToken = default)
    {
        var command = DeserializeCommand(request.Payload);
        var context = _aggregateStore is not null
            ? request.ToContext(_aggregateStore)
            : request.ToContext();
        return await _inner.ExecuteAsync(command, context, cancellationToken);
    }

    private static TCommand DeserializeCommand(object payload)
    {
        if (payload is TCommand typed)
            return typed;

        if (payload is JsonElement json)
        {
            return JsonSerializer.Deserialize<TCommand>(json.GetRawText(),
                SerializerOptions)
                ?? throw new InvalidOperationException(
                    $"Failed to deserialize payload to {typeof(TCommand).Name}.");
        }

        // Try round-trip serialization for anonymous/dynamic types
        var serialized = JsonSerializer.Serialize(payload);
        return JsonSerializer.Deserialize<TCommand>(serialized, SerializerOptions)
            ?? throw new InvalidOperationException(
                $"Failed to deserialize payload to {typeof(TCommand).Name}.");
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
}
