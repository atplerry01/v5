namespace Whycespace.Shared.Contracts.Runtime.OutboundEffects;

/// <summary>
/// R3.B.1 / R-OUT-EFF-TIMEOUT-02 — per-provider typed options. All fields are
/// REQUIRED at composition; the dispatcher throws
/// <c>OutboundEffectErrors.OptionsMissing</c> when an intent references an
/// unconfigured provider.
/// </summary>
public sealed record OutboundEffectOptions
{
    public required string ProviderId { get; init; }

    /// <summary>Per-attempt deadline; linked CTS from caller token.</summary>
    public required int DispatchTimeoutMs { get; init; }

    /// <summary>Cumulative retry budget across all attempts.</summary>
    public required int TotalBudgetMs { get; init; }

    /// <summary>Dispatched → Acknowledged deadline (reconciliation trigger).</summary>
    public required int AckTimeoutMs { get; init; }

    /// <summary>Acknowledged → Finalized deadline (reconciliation trigger).</summary>
    public required int FinalityWindowMs { get; init; }

    /// <summary>Retry cap. Exhaustion emits <c>OutboundEffectRetryExhaustedEvent</c>.</summary>
    public required int MaxAttempts { get; init; }

    public int BaseBackoffMs { get; init; } = 200;
    public int MaxBackoffMs { get; init; } = 30_000;
    public int JitterMs { get; init; } = 100;
    public int BreakerFailureThreshold { get; init; } = 5;
    public int BreakerWindowSeconds { get; init; } = 30;
}
