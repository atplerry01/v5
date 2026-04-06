namespace Whycespace.Domain.CoreSystem.State.SystemState;

/// <summary>
/// Specification: all recorded state validations must pass for system coherence.
/// Pure predicate — no side effects.
/// </summary>
public sealed class StateCoherenceSpecification
{
    public bool IsSatisfiedBy(IReadOnlyList<StateValidationResult> validations) =>
        validations.Count > 0 && validations.All(v => v.IsValid);
}
