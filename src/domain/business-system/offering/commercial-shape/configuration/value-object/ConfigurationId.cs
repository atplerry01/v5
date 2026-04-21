using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Configuration;

public readonly record struct ConfigurationId
{
    public Guid Value { get; }

    public ConfigurationId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ConfigurationId cannot be empty.");
        Value = value;
    }
}
