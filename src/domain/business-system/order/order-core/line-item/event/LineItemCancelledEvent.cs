using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.LineItem;

public sealed record LineItemCancelledEvent(
    [property: JsonPropertyName("AggregateId")] LineItemId LineItemId) : DomainEvent;
