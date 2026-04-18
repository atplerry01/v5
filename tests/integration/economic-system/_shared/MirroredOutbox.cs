using Whycespace.Shared.Contracts.Infrastructure.Messaging;
using Whycespace.Tests.Integration.Setup;

namespace Whycespace.Tests.Integration.EconomicSystem.Shared;

/// <summary>
/// phase5-operational-activation + phase6-hardening: real-Postgres outbox
/// with an <see cref="InMemoryOutbox"/> observer attached.
///
/// Primary durability path: <see cref="Whycespace.Platform.Host.Adapters.PostgresOutboxAdapter"/>.
/// If the real enqueue throws (saturation, transport failure, ...) the
/// mirror is NOT written — the test MUST see the real failure, not a
/// silent in-memory success. The mirror preserves the existing
/// <c>host.Outbox.Batches</c> inspection surface.
/// </summary>
internal sealed class MirroredOutbox : IOutbox
{
    private readonly IOutbox _real;
    private readonly InMemoryOutbox _mirror;

    public MirroredOutbox(IOutbox real, InMemoryOutbox mirror)
    {
        ArgumentNullException.ThrowIfNull(real);
        ArgumentNullException.ThrowIfNull(mirror);
        _real = real;
        _mirror = mirror;
    }

    public InMemoryOutbox Mirror => _mirror;

    public async Task EnqueueAsync(Guid correlationId, Guid aggregateId, IReadOnlyList<object> events, string topic, CancellationToken cancellationToken = default)
    {
        await _real.EnqueueAsync(correlationId, aggregateId, events, topic, cancellationToken);
        await _mirror.EnqueueAsync(correlationId, aggregateId, events, topic, cancellationToken);
    }
}
