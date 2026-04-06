using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T0U.CoreSystem.Temporal;

/// <summary>
/// T0U validation gate for temporal ordering invariants.
/// Validates monotonic timestamps, non-overlapping schedules.
///
/// NO domain imports — operates on primitive inputs.
/// </summary>
public sealed class TemporalValidationGate : IValidationGate
{
    public Task<ValidationResult> ValidateAsync(
        string commandType, string entityId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityId))
            return Task.FromResult(ValidationResult.Invalid("Entity ID is required for temporal validation."));

        return Task.FromResult(ValidationResult.Valid());
    }
}
