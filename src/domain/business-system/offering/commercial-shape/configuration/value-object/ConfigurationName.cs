using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Configuration;

public readonly record struct ConfigurationName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public ConfigurationName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "ConfigurationName must not be empty.");

        var trimmed = value!.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"ConfigurationName exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
