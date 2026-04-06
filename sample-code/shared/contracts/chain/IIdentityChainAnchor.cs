namespace Whycespace.Shared.Contracts.Chain;

/// <summary>
/// Contract for anchoring identity-critical events to WhyceChain.
/// Implementations live in T0U engine — runtime middleware calls this via DI.
/// </summary>
public interface IIdentityChainAnchor
{
    /// <summary>
    /// Returns true if this event type MUST be anchored to the chain.
    /// </summary>
    bool MustAnchor(string eventType);

    /// <summary>
    /// Anchor an identity-critical event to the WhyceChain.
    /// FAILURE to anchor → command MUST fail. No async inconsistency.
    /// </summary>
    Task<ChainWriteResult> AnchorAsync(AnchorRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Request to anchor an identity-critical event to WhyceChain.
/// </summary>
public sealed record AnchorRequest
{
    public required string EventId { get; init; }
    public required string AggregateId { get; init; }
    public required string EventType { get; init; }
    public required object EventData { get; init; }
    public string? PolicyDecisionHash { get; init; }
    public string? ExecutionHash { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
}
