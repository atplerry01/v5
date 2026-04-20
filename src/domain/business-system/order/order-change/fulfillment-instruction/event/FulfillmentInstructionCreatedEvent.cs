using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.FulfillmentInstruction;

public sealed record FulfillmentInstructionCreatedEvent(
    FulfillmentInstructionId FulfillmentInstructionId,
    OrderRef Order,
    LineItemRef? LineItem,
    FulfillmentDirective Directive);
