using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.LineItem;

public sealed record LineItemCreatedEvent(
    [property: JsonPropertyName("AggregateId")] LineItemId LineItemId,
    OrderRef Order,
    LineItemSubjectRef Subject,
    LineQuantity Quantity) : DomainEvent;
