using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Segment;

public readonly record struct SegmentName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public SegmentName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "SegmentName must not be empty.");

        var trimmed = value.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"SegmentName exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
