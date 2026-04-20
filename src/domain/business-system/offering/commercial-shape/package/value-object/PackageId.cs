namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Package;

public readonly record struct PackageId
{
    public Guid Value { get; }

    public PackageId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PackageId value must not be empty.", nameof(value));

        Value = value;
    }
}
