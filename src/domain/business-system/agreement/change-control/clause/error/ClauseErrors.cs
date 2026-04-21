using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Clause;

public static class ClauseErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("ClauseId is required and must not be empty.");

    public static DomainException InvalidClauseType()
        => new DomainInvariantViolationException("ClauseType must be a valid defined value.");

    public static DomainException AlreadySuperseded(ClauseId id)
        => new DomainInvariantViolationException($"Clause '{id.Value}' has already been superseded.");

    public static DomainException InvalidStateTransition(ClauseStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Clause has already been initialized.");
}
