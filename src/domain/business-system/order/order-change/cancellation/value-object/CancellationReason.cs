using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Cancellation;

public readonly record struct CancellationReason
{
    public const int MaxLength = 1000;

    public string Value { get; }

    public CancellationReason(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "CancellationReason must not be empty.");

        var trimmed = value!.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"CancellationReason exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
