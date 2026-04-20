namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Configuration;

public readonly record struct ConfigurationId
{
    public Guid Value { get; }

    public ConfigurationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ConfigurationId value must not be empty.", nameof(value));

        Value = value;
    }
}
