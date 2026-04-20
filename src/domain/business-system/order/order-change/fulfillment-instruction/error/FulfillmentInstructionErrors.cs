namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.FulfillmentInstruction;

public static class FulfillmentInstructionErrors
{
    public static FulfillmentInstructionDomainException MissingId()
        => new("FulfillmentInstructionId is required and must not be empty.");

    public static FulfillmentInstructionDomainException MissingOrderRef()
        => new("FulfillmentInstruction must reference an order.");

    public static FulfillmentInstructionDomainException InvalidStateTransition(FulfillmentInstructionStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static FulfillmentInstructionDomainException AlreadyTerminal(FulfillmentInstructionId id, FulfillmentInstructionStatus status)
        => new($"FulfillmentInstruction '{id.Value}' is already terminal ({status}).");
}

public sealed class FulfillmentInstructionDomainException : Exception
{
    public FulfillmentInstructionDomainException(string message) : base(message) { }
}
