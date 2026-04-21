using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceDefinition;

public readonly record struct ServiceDefinitionName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public ServiceDefinitionName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "ServiceDefinitionName must not be empty.");

        var trimmed = value!.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"ServiceDefinitionName exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
