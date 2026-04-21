using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.ServiceOffering;

public readonly record struct ServiceOfferingName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public ServiceOfferingName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "ServiceOfferingName must not be empty.");

        var trimmed = value!.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"ServiceOfferingName exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
