using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Resource.Utilization;

public sealed class ResourceInventoryAggregate : AggregateRoot
{
    public ResourceId ResourceId { get; private set; } = null!;
    public ResourceType ResourceType { get; private set; } = null!;
    public CapacityValue TotalCapacity { get; private set; } = null!;
    public CapacityValue AvailableCapacity { get; private set; } = null!;

    private ResourceInventoryAggregate() { }

    public ResourceInventoryAggregate(Guid inventoryId, ResourceId resourceId, ResourceType resourceType)
    {
        Id = inventoryId;
        ResourceId = resourceId ?? throw new ArgumentNullException(nameof(resourceId));
        ResourceType = resourceType ?? throw new ArgumentNullException(nameof(resourceType));
        TotalCapacity = CapacityValue.Zero;
        AvailableCapacity = CapacityValue.Zero;
    }

    public void AddCapacity(CapacityValue amount)
    {
        ArgumentNullException.ThrowIfNull(amount);

        if (amount.Value <= 0)
            throw new ArgumentException("Capacity to add must be greater than zero.", nameof(amount));

        TotalCapacity = TotalCapacity.Add(amount);
        AvailableCapacity = AvailableCapacity.Add(amount);

        RaiseDomainEvent(new ResourceCapacityAddedEvent(
            ResourceId.Value,
            amount.Value,
            TotalCapacity.Value,
            AvailableCapacity.Value
        ));
    }

    public void ReduceCapacity(CapacityValue amount)
    {
        ArgumentNullException.ThrowIfNull(amount);

        if (amount.Value <= 0)
            throw new ArgumentException("Capacity to reduce must be greater than zero.", nameof(amount));

        AvailableCapacity = AvailableCapacity.Subtract(amount);
        TotalCapacity = TotalCapacity.Subtract(amount);

        RaiseDomainEvent(new ResourceCapacityReducedEvent(
            ResourceId.Value,
            amount.Value,
            TotalCapacity.Value,
            AvailableCapacity.Value
        ));
    }
}
