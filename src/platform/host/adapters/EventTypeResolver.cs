using System.Text.Json;

namespace Whyce.Platform.Host.Adapters;

/// <summary>
/// Resolves event type names back to domain event objects when loading from
/// the Postgres event store. Maps stored event_type strings to concrete types.
/// </summary>
internal static class EventTypeResolver
{
    private static readonly Dictionary<string, Type> KnownTypes = new()
    {
        ["TodoCreatedEvent"] = typeof(Whycespace.Domain.OperationalSystem.Sandbox.Todo.TodoCreatedEvent),
        ["TodoUpdatedEvent"] = typeof(Whycespace.Domain.OperationalSystem.Sandbox.Todo.TodoUpdatedEvent),
        ["TodoCompletedEvent"] = typeof(Whycespace.Domain.OperationalSystem.Sandbox.Todo.TodoCompletedEvent),
    };

    public static object? Resolve(string eventType, string payload)
    {
        if (!KnownTypes.TryGetValue(eventType, out var type))
            return null;

        return JsonSerializer.Deserialize(payload, type);
    }
}
