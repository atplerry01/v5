using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.QuoteBasis;

public readonly record struct QuoteBasisContext
{
    public const int MaxLength = 2000;

    public string Value { get; }

    public QuoteBasisContext(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "QuoteBasisContext must not be empty.");
        Guard.Against(value!.Length > MaxLength, $"QuoteBasisContext exceeds {MaxLength} characters.");

        Value = value;
    }
}
