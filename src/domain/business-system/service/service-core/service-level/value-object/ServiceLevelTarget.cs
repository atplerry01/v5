using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceLevel;

public readonly record struct ServiceLevelTarget
{
    public const int MaxLength = 500;

    public string Descriptor { get; }

    public ServiceLevelTarget(string descriptor)
    {
        Guard.Against(string.IsNullOrWhiteSpace(descriptor), "ServiceLevelTarget descriptor must not be empty.");

        var trimmed = descriptor!.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"ServiceLevelTarget descriptor exceeds {MaxLength} characters.");

        Descriptor = trimmed;
    }
}
