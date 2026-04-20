namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceLevel;

public readonly record struct ServiceLevelTarget
{
    public const int MaxLength = 500;

    public string Descriptor { get; }

    public ServiceLevelTarget(string descriptor)
    {
        if (string.IsNullOrWhiteSpace(descriptor))
            throw new ArgumentException("ServiceLevelTarget descriptor must not be empty.", nameof(descriptor));

        var trimmed = descriptor.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"ServiceLevelTarget descriptor exceeds {MaxLength} characters.", nameof(descriptor));

        Descriptor = trimmed;
    }
}
