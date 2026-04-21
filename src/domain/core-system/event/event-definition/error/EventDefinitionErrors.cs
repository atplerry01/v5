using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Event.EventDefinition;

public static class EventDefinitionErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("EventDefinitionId is required and must not be empty.");

    public static DomainException MissingSchema()
        => new DomainInvariantViolationException("Event definition must include a valid schema.");

    public static DomainException InvalidStateTransition(EventDefinitionStatus current, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{current}'.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("EventDefinition has already been initialized.");
}
