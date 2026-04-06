namespace Whycespace.Shared.Contracts.Engine;

/// <summary>
/// Short-circuit decorator for IValidationGate. Skips validation
/// for command types that are known to be exempt from a specific gate.
///
/// Example: read-only queries don't need financial control validation.
/// The exempt set is configured at startup and is immutable at runtime.
/// Deterministic: exemption list is static configuration, not runtime state.
/// </summary>
public sealed class ShortCircuitValidationGate : IValidationGate
{
    private readonly IValidationGate _inner;
    private readonly HashSet<string> _exemptCommandTypes;

    public ShortCircuitValidationGate(IValidationGate inner, IEnumerable<string> exemptCommandTypes)
    {
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentNullException.ThrowIfNull(exemptCommandTypes);
        _inner = inner;
        _exemptCommandTypes = new HashSet<string>(exemptCommandTypes, StringComparer.OrdinalIgnoreCase);
    }

    public Task<ValidationResult> ValidateAsync(
        string commandType, string entityId, CancellationToken cancellationToken = default)
    {
        if (_exemptCommandTypes.Contains(commandType))
            return Task.FromResult(ValidationResult.Valid());

        return _inner.ValidateAsync(commandType, entityId, cancellationToken);
    }
}
