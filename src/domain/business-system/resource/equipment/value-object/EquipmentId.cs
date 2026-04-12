namespace Whycespace.Domain.BusinessSystem.Resource.Equipment;

public readonly record struct EquipmentId
{
    public Guid Value { get; }

    public EquipmentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EquipmentId value must not be empty.", nameof(value));

        Value = value;
    }
}
