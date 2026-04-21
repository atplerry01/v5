using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.FulfillmentInstruction;

public sealed record FulfillmentInstructionRevokedEvent(
    [property: JsonPropertyName("AggregateId")] FulfillmentInstructionId FulfillmentInstructionId,
    DateTimeOffset RevokedAt) : DomainEvent;
