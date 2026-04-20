namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// R1 §5 — state-transition eligibility seam. Consulted by
/// <c>ExecutionGuardMiddleware</c> immediately before engine dispatch to verify
/// the current aggregate state permits the requested transition.
///
/// This is DISTINCT from:
/// - Semantic validation (<see cref="ISemanticValidator{TCommand}"/>) — pre-policy, shape-only
/// - Business invariants — enforced inside the aggregate on write
///
/// State-transition failures produce <see cref="ValidationFailureCategory.StateTransition"/>.
///
/// The <paramref name="currentState"/> snapshot is typed as <c>object</c> because
/// runtime layer must not reference concrete domain types (guard R11). Implementations
/// live adjacent to their aggregate in the domain/engine layer and cast internally.
/// </summary>
public interface IStateTransitionValidator
{
    /// <summary>Command type this validator is bound to. Used by the middleware to select the right validator.</summary>
    Type CommandType { get; }

    Task<ValidationResult> ValidateAsync(
        object command,
        object? currentState,
        CancellationToken cancellationToken = default);
}
