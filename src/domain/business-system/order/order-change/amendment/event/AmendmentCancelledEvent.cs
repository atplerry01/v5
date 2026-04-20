namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Amendment;

public sealed record AmendmentCancelledEvent(AmendmentId AmendmentId, DateTimeOffset CancelledAt);
