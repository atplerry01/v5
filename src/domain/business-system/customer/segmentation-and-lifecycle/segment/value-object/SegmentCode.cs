using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Segment;

public readonly record struct SegmentCode
{
    public const int MaxLength = 64;

    public string Value { get; }

    public SegmentCode(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "SegmentCode must not be empty.");

        var trimmed = value.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"SegmentCode exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
