namespace Whycespace.Domain.BusinessSystem.Resource.Facility;

public readonly record struct FacilityId
{
    public Guid Value { get; }

    public FacilityId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("FacilityId value must not be empty.", nameof(value));

        Value = value;
    }
}
