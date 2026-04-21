using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Amendment;

public readonly record struct AmendmentReason
{
    public const int MaxLength = 1000;

    public string Value { get; }

    public AmendmentReason(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "AmendmentReason must not be empty.");

        var trimmed = value!.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"AmendmentReason exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
