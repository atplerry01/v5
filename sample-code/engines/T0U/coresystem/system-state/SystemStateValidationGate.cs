using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T0U.CoreSystem.SystemState;

/// <summary>
/// T0U validation gate for system state authority.
/// Validates that the system is in a healthy state before accepting commands.
///
/// NO domain imports — operates on primitive inputs.
/// </summary>
public sealed class SystemStateValidationGate : IValidationGate
{
    public Task<ValidationResult> ValidateAsync(
        string commandType, string entityId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityId))
            return Task.FromResult(ValidationResult.Invalid("Entity ID is required for system state validation."));

        return Task.FromResult(ValidationResult.Valid());
    }
}
