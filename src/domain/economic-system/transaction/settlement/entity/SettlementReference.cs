using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Settlement;

/// <summary>
/// Immutable reference to the external execution record bound to a settlement.
/// Captures the provider, the external transaction id (assigned by the rail on
/// completion), and opaque metadata. The domain treats this as an append-only
/// attestation surface — once attached on completion, it cannot be rewritten.
/// </summary>
public sealed class SettlementReference : Entity
{
    public SettlementProvider Provider { get; private set; }
    public SettlementReferenceId ExternalTransactionId { get; private set; }
    public string Metadata { get; private set; } = string.Empty;

    private SettlementReference() { }

    internal static SettlementReference Create(
        SettlementProvider provider,
        SettlementReferenceId externalTransactionId,
        string? metadata)
    {
        if (string.IsNullOrWhiteSpace(provider.Value))
            throw new ArgumentException("Provider must be set.", nameof(provider));
        if (string.IsNullOrWhiteSpace(externalTransactionId.Value))
            throw new ArgumentException("ExternalTransactionId must be set.", nameof(externalTransactionId));

        return new SettlementReference
        {
            Provider = provider,
            ExternalTransactionId = externalTransactionId,
            Metadata = metadata ?? string.Empty
        };
    }
}
