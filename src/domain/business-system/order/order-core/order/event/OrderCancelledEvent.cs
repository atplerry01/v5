using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Order;

public sealed record OrderCancelledEvent(
    [property: JsonPropertyName("AggregateId")] OrderId OrderId,
    DateTimeOffset CancelledAt) : DomainEvent;
