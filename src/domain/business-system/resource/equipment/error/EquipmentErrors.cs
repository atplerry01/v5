namespace Whycespace.Domain.BusinessSystem.Resource.Equipment;

public static class EquipmentErrors
{
    public static EquipmentDomainException MissingId()
        => new("EquipmentId is required and must not be empty.");

    public static EquipmentDomainException AlreadyActive(EquipmentId id)
        => new($"Equipment '{id.Value}' is already active.");

    public static EquipmentDomainException AlreadyRetired(EquipmentId id)
        => new($"Equipment '{id.Value}' has already been retired.");

    public static EquipmentDomainException InvalidStateTransition(EquipmentStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class EquipmentDomainException : Exception
{
    public EquipmentDomainException(string message) : base(message) { }
}
