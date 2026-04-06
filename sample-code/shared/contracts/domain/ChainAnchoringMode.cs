namespace Whycespace.Shared.Contracts.Domain;

/// <summary>
/// Controls how chain anchoring failures are handled.
/// Determined by policy decision at evaluation time.
/// </summary>
public enum ChainAnchoringMode
{
    /// <summary>
    /// Chain anchoring failure causes the entire operation to fail.
    /// Used for critical operations: financial transactions, policy changes, identity mutations.
    /// </summary>
    Strict,

    /// <summary>
    /// Chain anchoring failure is logged but does not fail the operation.
    /// Used for non-critical operations: analytics, projections, read-model updates.
    /// </summary>
    Async,

    /// <summary>
    /// Chain anchoring is skipped entirely.
    /// Used for pure validation/read-only operations (e.g., federation checks).
    /// Must be explicitly set — default is Async.
    /// </summary>
    None
}

public sealed class ChainAnchorFailureException : Exception
{
    public string DecisionHash { get; }
    public string CorrelationId { get; }

    public ChainAnchorFailureException(string decisionHash, string correlationId, string message)
        : base(message)
    {
        DecisionHash = decisionHash;
        CorrelationId = correlationId;
    }
}
