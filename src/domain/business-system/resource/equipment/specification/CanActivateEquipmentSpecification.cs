namespace Whycespace.Domain.BusinessSystem.Resource.Equipment;

public sealed class CanActivateEquipmentSpecification
{
    public bool IsSatisfiedBy(EquipmentStatus status)
    {
        return status == EquipmentStatus.Pending;
    }
}
