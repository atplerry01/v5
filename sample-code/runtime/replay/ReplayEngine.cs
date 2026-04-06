using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Persistence;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.Replay;

public enum ReplayMode
{
    /// <summary>Replay a single aggregate stream.</summary>
    Stream,
    /// <summary>Replay all events from a point in time.</summary>
    Global
}

public sealed record ReplayRequest
{
    public required ReplayMode Mode { get; init; }
    public string? StreamId { get; init; }
    public long FromVersion { get; init; }
    public DateTimeOffset? After { get; init; }
}

public sealed record ReplayResult
{
    public required bool Success { get; init; }
    public required long EventsReplayed { get; init; }
    public required TimeSpan Elapsed { get; init; }
    public string? ErrorMessage { get; init; }

    public static ReplayResult Ok(long eventsReplayed, TimeSpan elapsed) =>
        new() { Success = true, EventsReplayed = eventsReplayed, Elapsed = elapsed };

    public static ReplayResult Fail(string error, long eventsReplayed, TimeSpan elapsed) =>
        new() { Success = false, EventsReplayed = eventsReplayed, Elapsed = elapsed, ErrorMessage = error };
}

/// <summary>
/// Replays event streams from the event store through consumers and projections.
/// Used for:
///   - Rebuilding read models after schema changes
///   - Verifying deterministic output
///   - Recovering from projection failures
///   - Audit and compliance verification
/// </summary>
public sealed class ReplayEngine
{
    private readonly IEventStore _eventStore;
    private readonly ReadModelUpdater? _readModelUpdater;
    private readonly IClock _clock;
    private readonly List<Func<RuntimeEvent, CancellationToken, Task>> _replayConsumers = new();

    public ReplayEngine(IEventStore eventStore, IClock? clock = null)
    {
        _eventStore = eventStore;
        _clock = clock ?? SystemClock.Instance;
    }

    public ReplayEngine(IEventStore eventStore, ReadModelUpdater readModelUpdater, IClock? clock = null)
        : this(eventStore, clock)
    {
        _readModelUpdater = readModelUpdater;
    }

    /// <summary>
    /// Registers a consumer that receives events during replay.
    /// </summary>
    public void OnReplay(Func<RuntimeEvent, CancellationToken, Task> consumer)
    {
        ArgumentNullException.ThrowIfNull(consumer);
        _replayConsumers.Add(consumer);
    }

    public async Task<ReplayResult> ReplayAsync(ReplayRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var started = _clock.UtcNowOffset;
        long replayed = 0;

        try
        {
            var events = await LoadEventsAsync(request, cancellationToken);

            foreach (var @event in events)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Mark as replay so consumers can distinguish
                var replayEvent = @event with
                {
                    Headers = new Dictionary<string, string>(@event.Headers)
                    {
                        [ReplayHeaders.IsReplay] = "true",
                        [ReplayHeaders.ReplayTimestamp] = _clock.UtcNowOffset.ToString("O")
                    }
                };

                // Route to registered replay consumers
                foreach (var consumer in _replayConsumers)
                {
                    await consumer(replayEvent, cancellationToken);
                }

                // Route to read model updater if available
                if (_readModelUpdater is not null)
                {
                    await _readModelUpdater.UpdateAsync(replayEvent, cancellationToken);
                }

                replayed++;
            }

            var elapsed = _clock.UtcNowOffset - started;
            return ReplayResult.Ok(replayed, elapsed);
        }
        catch (OperationCanceledException)
        {
            var elapsed = _clock.UtcNowOffset - started;
            return ReplayResult.Fail("Replay cancelled.", replayed, elapsed);
        }
        catch (Exception ex)
        {
            var elapsed = _clock.UtcNowOffset - started;
            return ReplayResult.Fail($"Replay failed: {ex.Message}", replayed, elapsed);
        }
    }

    private async Task<IReadOnlyList<RuntimeEvent>> LoadEventsAsync(
        ReplayRequest request, CancellationToken cancellationToken)
    {
        return request.Mode switch
        {
            ReplayMode.Stream when request.StreamId is not null =>
                await _eventStore.ReadStreamAsync(request.StreamId, request.FromVersion, cancellationToken),

            ReplayMode.Global =>
                await _eventStore.ReadAllAsync(request.After, cancellationToken),

            _ => throw new ArgumentException("Stream replay requires a StreamId.", nameof(request))
        };
    }

    public static class ReplayHeaders
    {
        public const string IsReplay = "x-replay";
        public const string ReplayTimestamp = "x-replay-timestamp";
    }
}
