using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Runtime.EventFabric;

/// <summary>
/// Event Fabric Pipeline — orchestrates fabric stages with error handling.
/// </summary>
public sealed class EventFabricPipeline
{
    private readonly IEventFabric _fabric;

    public EventFabricPipeline(IEventFabric fabric)
    {
        _fabric = fabric;
    }

    public async Task<EventFabricResult> ExecuteAsync(
        IReadOnlyList<object> domainEvents, CommandContext context)
    {
        if (domainEvents.Count == 0)
            return EventFabricResult.Empty();

        try
        {
            await _fabric.ProcessAsync(domainEvents, context);
            return EventFabricResult.Success(domainEvents.Count);
        }
        catch (Exception ex)
        {
            return EventFabricResult.Failure(ex.Message);
        }
    }
}

public sealed record EventFabricResult
{
    public bool IsSuccess { get; init; }
    public int EventCount { get; init; }
    public string? Error { get; init; }

    public static EventFabricResult Success(int eventCount) =>
        new() { IsSuccess = true, EventCount = eventCount };
    public static EventFabricResult Failure(string error) =>
        new() { IsSuccess = false, Error = error };
    public static EventFabricResult Empty() =>
        new() { IsSuccess = true, EventCount = 0 };
}
