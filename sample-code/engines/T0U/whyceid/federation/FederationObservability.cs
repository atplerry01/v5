namespace Whycespace.Engines.T0U.WhyceId.Federation;

/// <summary>
/// FIX 5 — Observability signals emitted by federation middleware.
///
/// Signals:
///   - federation.validation.result
///   - federation.policy.decision
///   - federation.propagation.result
///   - federation.chain.anchor.status
///
/// Attached to runtime telemetry via CommandContext properties.
/// Non-persistent — used for tracing and metrics only.
/// </summary>
public static class FederationObservability
{
    public static class Keys
    {
        public const string ValidationResult = "Federation.Signal.Validation";
        public const string PolicyDecision = "Federation.Signal.Policy";
        public const string PropagationResult = "Federation.Signal.Propagation";
        public const string ChainAnchorStatus = "Federation.Signal.ChainAnchor";
    }

    public sealed record ValidationSignal(
        bool IsValid,
        int ReasonCount,
        string? FirstReason);

    public sealed record PolicyDecisionSignal(
        string Outcome,
        string Source,
        string? Reason);

    public sealed record PropagationSignal(
        string Status,
        decimal EffectiveTrust,
        string? BlockReason);

    public sealed record ChainAnchorSignal(
        string EventType,
        bool Anchored,
        string? BlockId);
}
