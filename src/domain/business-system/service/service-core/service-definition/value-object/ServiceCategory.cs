using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceDefinition;

public readonly record struct ServiceCategory
{
    public const int MaxLength = 100;

    public string Value { get; }

    public ServiceCategory(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "ServiceCategory must not be empty.");

        var trimmed = value!.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"ServiceCategory exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
