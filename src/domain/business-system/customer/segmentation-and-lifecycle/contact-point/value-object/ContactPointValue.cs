using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.ContactPoint;

public readonly record struct ContactPointValue
{
    public const int MaxLength = 512;

    public string Value { get; }

    public ContactPointValue(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "ContactPointValue must not be empty.");

        var trimmed = value.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"ContactPointValue exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
