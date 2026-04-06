namespace Whycespace.Domain.SharedKernel.Primitives.Kernel;

/// <summary>
/// Thrown when a domain aggregate invariant is violated.
/// Distinct from DomainException (business rule violation) — this represents
/// an attempt to put an aggregate into a structurally invalid state.
/// </summary>
public sealed class DomainInvariantViolationException : DomainException
{
    public string AggregateType { get; }
    public Guid? AggregateId { get; }

    public DomainInvariantViolationException(
        string aggregateType,
        string invariant,
        string message,
        Guid? aggregateId = null)
        : base($"INVARIANT_VIOLATION_{invariant}", $"[{aggregateType}] {message}")
    {
        AggregateType = aggregateType;
        AggregateId = aggregateId;
    }

    public static DomainInvariantViolationException InvalidStateTransition(
        string aggregateType,
        string fromState,
        string toState,
        Guid? aggregateId = null)
        => new(
            aggregateType,
            "INVALID_STATE_TRANSITION",
            $"Cannot transition from '{fromState}' to '{toState}'.",
            aggregateId);

    public static DomainInvariantViolationException RequiredField(
        string aggregateType,
        string fieldName,
        Guid? aggregateId = null)
        => new(
            aggregateType,
            "REQUIRED_FIELD",
            $"'{fieldName}' is required and cannot be null or empty.",
            aggregateId);

    public static DomainInvariantViolationException TerminalState(
        string aggregateType,
        string currentState,
        string attemptedAction,
        Guid? aggregateId = null)
        => new(
            aggregateType,
            "TERMINAL_STATE",
            $"Cannot '{attemptedAction}' — aggregate is in terminal state '{currentState}'.",
            aggregateId);

    public static DomainInvariantViolationException ConsistencyViolation(
        string aggregateType,
        string message,
        Guid? aggregateId = null)
        => new(
            aggregateType,
            "CONSISTENCY",
            message,
            aggregateId);
}
