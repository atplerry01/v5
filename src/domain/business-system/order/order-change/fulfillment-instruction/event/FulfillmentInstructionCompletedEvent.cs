using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.FulfillmentInstruction;

public sealed record FulfillmentInstructionCompletedEvent(
    [property: JsonPropertyName("AggregateId")] FulfillmentInstructionId FulfillmentInstructionId,
    DateTimeOffset CompletedAt) : DomainEvent;
