namespace Whycespace.Domain.BusinessSystem.Logistic.Dispatch;

public sealed record DispatchCreatedEvent(
    DispatchId DispatchId,
    ShipmentReference ShipmentReference);
