using Whycespace.Shared.Contracts.Infrastructure.Storage;

namespace Whycespace.Shared.Contracts.Chain;

/// <summary>
/// Contract for anchoring ledger-critical and settlement-critical events to WhyceChain.
/// Implementations live in T0U engine — runtime middleware calls this via DI.
///
/// E17.3.9: STRICT mode for ledger and settlement events.
/// Anchor failure → operation MUST fail. No async inconsistency.
/// </summary>
public interface ILedgerChainAnchor
{
    /// <summary>
    /// Returns true if this event type MUST be anchored to the chain.
    /// Covers ledger entry and settlement events.
    /// </summary>
    bool MustAnchor(string eventType);

    /// <summary>
    /// Anchor a ledger-critical event to the WhyceChain.
    /// STRICT: failure to anchor → command MUST fail.
    /// </summary>
    Task<ChainWriteResult> AnchorAsync(AnchorRequest request, CancellationToken cancellationToken = default);
}