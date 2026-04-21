using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.LineItem;

public sealed record LineItemUpdatedEvent(
    [property: JsonPropertyName("AggregateId")] LineItemId LineItemId,
    LineQuantity Quantity) : DomainEvent;
