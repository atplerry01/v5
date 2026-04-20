namespace Whycespace.Runtime.OutboundEffects;

/// <summary>
/// R3.B.1 — options for <see cref="OutboundEffectRelay"/>. Host composition
/// supplies a stable <see cref="HostId"/> (machine-scoped) for the
/// <c>claimed_by</c> column, a batch size, and a poll cadence used by the
/// hosted-service shell.
/// </summary>
public sealed record OutboundEffectRelayOptions
{
    public required string HostId { get; init; }
    public int BatchSize { get; init; } = 32;
    public int PollIntervalMs { get; init; } = 500;
}
