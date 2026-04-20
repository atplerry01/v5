using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.LineItem;

public sealed record LineItemCreatedEvent(
    LineItemId LineItemId,
    OrderRef Order,
    LineItemSubjectRef Subject,
    LineQuantity Quantity);
