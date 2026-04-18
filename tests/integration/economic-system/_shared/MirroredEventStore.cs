using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Tests.Integration.Setup;

namespace Whycespace.Tests.Integration.EconomicSystem.Shared;

/// <summary>
/// phase5-operational-activation + phase6-hardening: real-Postgres event
/// store with an <see cref="InMemoryEventStore"/> observer attached.
///
/// Primary durability path: the real Postgres adapter. If it throws, the
/// mirror is NOT updated — the test harness MUST NOT fall back to
/// in-memory state when real infra fails. The mirror preserves the
/// existing test inspection API (<c>AllEvents(Guid)</c>, <c>Versions(Guid)</c>)
/// without the certification suite having to know whether it is running
/// in in-memory or real mode.
/// </summary>
internal sealed class MirroredEventStore : IEventStore
{
    private readonly IEventStore _real;
    private readonly InMemoryEventStore _mirror;

    public MirroredEventStore(IEventStore real, InMemoryEventStore mirror)
    {
        ArgumentNullException.ThrowIfNull(real);
        ArgumentNullException.ThrowIfNull(mirror);
        _real = real;
        _mirror = mirror;
    }

    public InMemoryEventStore Mirror => _mirror;

    public async Task<IReadOnlyList<object>> LoadEventsAsync(Guid aggregateId, CancellationToken cancellationToken = default)
    {
        return await _real.LoadEventsAsync(aggregateId, cancellationToken);
    }

    public async Task AppendEventsAsync(Guid aggregateId, IReadOnlyList<IEventEnvelope> envelopes, int expectedVersion, CancellationToken cancellationToken = default)
    {
        await _real.AppendEventsAsync(aggregateId, envelopes, expectedVersion, cancellationToken);
        await _mirror.AppendEventsAsync(aggregateId, envelopes, expectedVersion, cancellationToken);
    }
}
