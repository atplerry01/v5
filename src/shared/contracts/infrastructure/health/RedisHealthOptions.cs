namespace Whyce.Shared.Contracts.Infrastructure.Health;

/// <summary>
/// phase1.5-S5.2.4 / HC-9 (REDIS-HEALTH-01): declared Redis health
/// thresholds. <c>PingTimeoutMs</c> bounds the per-probe round
/// trip; <c>DegradedLatencyThresholdMs</c> is the boundary above
/// which a successful but slow ping reports DEGRADED rather than
/// HEALTHY. Both values are conservative defaults sized for the
/// MI-1 distributed-execution-lock hot path: a 200 ms ceiling on
/// ping latency keeps the per-dispatch overhead bounded, and a
/// 1 s probe timeout prevents the health check itself from
/// becoming a saturation signal under a degraded Redis.
/// </summary>
public sealed record RedisHealthOptions(
    int PingTimeoutMs = 1000,
    int DegradedLatencyThresholdMs = 200);
