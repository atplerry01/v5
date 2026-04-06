using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Persistence;

namespace Whycespace.Runtime.Projection;

internal static class EnumerableExtensions
{
    /// <summary>
    /// Skips <paramref name="count"/> elements without int truncation.
    /// Safe for checkpoints exceeding int.MaxValue (2.1B+ events).
    /// </summary>
    public static IEnumerable<T> LongSkip<T>(this IEnumerable<T> source, long count)
    {
        long skipped = 0;
        foreach (var item in source)
        {
            if (skipped < count)
            {
                skipped++;
                continue;
            }
            yield return item;
        }
    }
}

public delegate Task ProjectionHandler(RuntimeEvent @event, IProjectionStore store, CancellationToken cancellationToken);

public sealed class ProjectionRegistration
{
    public required string ProjectionName { get; init; }
    public required string[] EventTypes { get; init; }
    public required ProjectionHandler Handler { get; init; }
}

public sealed class ProjectionEngine
{
    private readonly List<ProjectionRegistration> _registrations = new();
    private readonly IEventStore _eventStore;
    private readonly IProjectionStore _projectionStore;

    public ProjectionEngine(IEventStore eventStore, IProjectionStore projectionStore)
    {
        _eventStore = eventStore;
        _projectionStore = projectionStore;
    }

    public IReadOnlyList<ProjectionRegistration> Registrations => _registrations.AsReadOnly();

    public void Register(ProjectionRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);

        if (registration.EventTypes.Length == 0)
            throw new ArgumentException("Projection must handle at least one event type.", nameof(registration));

        _registrations.Add(registration);
    }

    public void Register(string projectionName, string[] eventTypes, ProjectionHandler handler)
    {
        Register(new ProjectionRegistration
        {
            ProjectionName = projectionName,
            EventTypes = eventTypes,
            Handler = handler
        });
    }

    public async Task ProjectAsync(CancellationToken cancellationToken = default)
    {
        foreach (var registration in _registrations)
        {
            var checkpoint = await _projectionStore.GetCheckpointAsync(
                registration.ProjectionName, cancellationToken);

            var events = await _eventStore.ReadAllAsync(cancellationToken: cancellationToken);

            // Use long-safe skip via LINQ — no (int) cast truncation
            long processed = checkpoint;

            foreach (var @event in events.LongSkip(checkpoint))
            {
                processed++;

                if (registration.EventTypes.Contains(@event.EventType))
                {
                    await registration.Handler(@event, _projectionStore, cancellationToken);
                }
            }

            if (processed > checkpoint)
            {
                await _projectionStore.SetCheckpointAsync(
                    registration.ProjectionName, processed, cancellationToken);
            }
        }
    }

    public async Task ProjectStreamAsync(string streamId, CancellationToken cancellationToken = default)
    {
        var events = await _eventStore.ReadStreamAsync(streamId, cancellationToken);

        foreach (var @event in events)
        {
            var matchingRegistrations = _registrations
                .Where(r => r.EventTypes.Contains(@event.EventType));

            foreach (var registration in matchingRegistrations)
            {
                await registration.Handler(@event, _projectionStore, cancellationToken);
            }
        }
    }
}
