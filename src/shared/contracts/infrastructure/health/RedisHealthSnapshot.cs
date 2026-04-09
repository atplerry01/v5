namespace Whyce.Shared.Contracts.Infrastructure.Health;

/// <summary>
/// phase1.5-S5.2.4 / HC-9 (REDIS-HEALTH-01): immutable view of
/// the most recent Redis ping observation. Sourced by the
/// canonical <c>RedisHealthSnapshotProvider</c> from
/// <c>IConnectionMultiplexer.IsConnected</c> and a single
/// <c>db.PingAsync()</c> probe. <c>PingLatencyMs</c> is null when
/// the multiplexer is not connected or the probe threw.
/// </summary>
public sealed record RedisHealthSnapshot(
    bool IsHealthy,
    bool IsConnected,
    long? PingLatencyMs,
    DateTimeOffset CheckedAtUtc);
