namespace Whycespace.Domain.EconomicSystem.Transaction.Settlement;

/// <summary>
/// Stateless domain service. Performs pure validation against settlements — it
/// never calls external APIs, never persists, never emits events. Its sole job
/// is to express settlement-wide invariants that do not belong to any single
/// aggregate instance (e.g., verifying that a completion carries a valid
/// external reference and is consistent with the source reference format).
/// </summary>
public sealed class SettlementService
{
    public bool ValidateExternalReference(SettlementReferenceId externalReferenceId)
    {
        if (string.IsNullOrWhiteSpace(externalReferenceId.Value)) return false;
        return externalReferenceId.Value.Length >= 3;
    }

    public bool ValidateConsistency(SettlementAggregate settlement)
    {
        if (settlement is null) return false;
        if (!new SettlementSpecification().IsSatisfiedBy(settlement)) return false;
        if (settlement.Status == SettlementStatus.Completed && settlement.Reference is null) return false;
        return true;
    }
}
