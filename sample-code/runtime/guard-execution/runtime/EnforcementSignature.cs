namespace Whycespace.Runtime.GuardExecution.Runtime;

/// <summary>
/// Lightweight enforcement trace carrier for logs, chain anchors, and telemetry.
/// Captures the deterministic identity of a single enforcement execution.
/// </summary>
public sealed class EnforcementSignature
{
    public required string CorrelationId { get; init; }
    public required string DecisionHash { get; init; }
    public required string GuardHash { get; init; }

    public override string ToString()
        => $"[corr={CorrelationId} decision={DecisionHash[..8]}.. guard={GuardHash[..8]}..]";
}
