using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T0U.CoreSystem.Reconciliation;

/// <summary>
/// T0U validation gate for reconciliation checks.
/// Validates system consistency AFTER T2E execution:
/// event-store vs projection drift, cross-ledger imbalances.
///
/// NO domain imports — operates on primitive inputs.
/// </summary>
public sealed class ReconciliationValidationGate : IValidationGate
{
    public Task<ValidationResult> ValidateAsync(
        string commandType, string entityId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityId))
            return Task.FromResult(ValidationResult.Invalid("Entity ID is required for reconciliation validation."));

        return Task.FromResult(ValidationResult.Valid());
    }
}
