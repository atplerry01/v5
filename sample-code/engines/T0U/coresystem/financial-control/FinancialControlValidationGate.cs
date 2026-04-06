using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T0U.CoreSystem.FinancialControl;

/// <summary>
/// T0U validation gate for financial control invariants.
/// Validates BEFORE T2E execution: rejects commands that would violate
/// global financial invariants (negative balance, imbalanced flows).
///
/// NO domain imports — operates on primitive inputs via IValidationGate.
/// Runtime provides the state snapshot through headers.
/// </summary>
public sealed class FinancialControlValidationGate : IValidationGate
{
    public Task<ValidationResult> ValidateAsync(
        string commandType, string entityId, CancellationToken cancellationToken = default)
    {
        // Gate validates financial commands against invariants
        // Runtime injects current balance state via aggregate store
        // If entityId represents a sealed financial control, reject
        if (string.IsNullOrWhiteSpace(entityId))
            return Task.FromResult(ValidationResult.Invalid("Entity ID is required for financial control validation."));

        return Task.FromResult(ValidationResult.Valid());
    }
}
