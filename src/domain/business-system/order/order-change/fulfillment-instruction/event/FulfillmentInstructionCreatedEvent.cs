using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.FulfillmentInstruction;

public sealed record FulfillmentInstructionCreatedEvent(
    [property: JsonPropertyName("AggregateId")] FulfillmentInstructionId FulfillmentInstructionId,
    OrderRef Order,
    LineItemRef? LineItem,
    FulfillmentDirective Directive) : DomainEvent;
