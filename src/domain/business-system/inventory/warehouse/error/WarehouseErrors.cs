namespace Whycespace.Domain.BusinessSystem.Inventory.Warehouse;

public static class WarehouseErrors
{
    public static WarehouseDomainException MissingId()
        => new("WarehouseId is required and must not be empty.");

    public static WarehouseDomainException InvalidCapacity()
        => new("Warehouse capacity must be positive.");

    public static WarehouseDomainException AlreadyDeactivated(WarehouseId id)
        => new($"Warehouse '{id.Value}' has already been deactivated.");

    public static WarehouseDomainException InvalidStateTransition(WarehouseStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class WarehouseDomainException : Exception
{
    public WarehouseDomainException(string message) : base(message) { }
}
