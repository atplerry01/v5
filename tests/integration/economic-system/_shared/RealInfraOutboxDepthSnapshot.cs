using Whycespace.Shared.Contracts.Infrastructure.Messaging;

namespace Whycespace.Tests.Integration.EconomicSystem.Shared;

/// <summary>
/// phase5-operational-activation + phase6-hardening: real-infra test seam.
/// Always-fresh, below-watermark <see cref="IOutboxDepthSnapshot"/> so the
/// PostgresOutboxAdapter's PC-3 / HC-1 refusal paths never trip during the
/// test lifecycle. Tests exercise the enqueue path; sampler/relay tuning is
/// out of scope for certification-level wiring.
/// </summary>
internal sealed class RealInfraOutboxDepthSnapshot : IOutboxDepthSnapshot
{
    public long CurrentDepth => 0;
    public double OldestPendingAgeSeconds => 0;
    public bool HasObservation => true;
    public DateTimeOffset LastUpdatedAt { get; private set; } = DateTimeOffset.MaxValue;

    public void Publish(long depth, double oldestPendingAgeSeconds) { }

    public bool IsFresh(DateTimeOffset now, int maxAgeSeconds) => true;
}
