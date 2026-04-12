namespace Whycespace.Domain.BusinessSystem.Resource.Utilization;

public sealed record UtilizationCreatedEvent(
    UtilizationId UtilizationId,
    ResourceReference ResourceReference,
    CapacityLimit CapacityLimit);
