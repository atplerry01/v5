namespace Whycespace.Domain.BusinessSystem.Resource.Equipment;

public sealed class CanRetireEquipmentSpecification
{
    public bool IsSatisfiedBy(EquipmentStatus status)
    {
        return status == EquipmentStatus.Active;
    }
}
