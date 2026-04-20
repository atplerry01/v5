namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// R1 §5 — semantic validation seam. Registered per command type; executed by
/// <c>ValidationMiddleware</c> after structural/schema checks pass.
/// Implementations validate cross-field invariants, formats, business ranges —
/// anything beyond raw shape but short of aggregate state.
///
/// Semantic validation is DISTINCT from:
/// - Input schema validation (structural, done inline in middleware)
/// - State-transition eligibility (<see cref="IStateTransitionValidator"/>)
/// - Business invariants (enforced by aggregates themselves)
///
/// Semantic failures produce <see cref="ValidationFailureCategory.Semantic"/>.
/// </summary>
public interface ISemanticValidator<in TCommand>
{
    Task<ValidationResult> ValidateAsync(TCommand command, CancellationToken cancellationToken = default);
}
