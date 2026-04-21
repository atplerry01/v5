using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Segment;

public readonly record struct SegmentCriteria
{
    public const int MaxLength = 2000;

    public string Value { get; }

    public SegmentCriteria(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "SegmentCriteria must not be empty.");
        Guard.Against(value.Length > MaxLength, $"SegmentCriteria exceeds {MaxLength} characters.");

        Value = value;
    }
}
