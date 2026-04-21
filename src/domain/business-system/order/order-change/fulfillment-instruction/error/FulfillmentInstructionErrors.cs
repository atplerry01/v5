using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.FulfillmentInstruction;

public static class FulfillmentInstructionErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("FulfillmentInstructionId is required and must not be empty.");

    public static DomainException MissingOrderRef()
        => new DomainInvariantViolationException("FulfillmentInstruction must reference an order.");

    public static DomainException InvalidStateTransition(FulfillmentInstructionStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException AlreadyTerminal(FulfillmentInstructionId id, FulfillmentInstructionStatus status)
        => new DomainInvariantViolationException($"FulfillmentInstruction '{id.Value}' is already terminal ({status}).");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("FulfillmentInstruction has already been initialized.");
}
