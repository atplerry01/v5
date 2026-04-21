using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.Quote;

public readonly record struct QuoteReference
{
    public const int MaxLength = 64;

    public string Value { get; }

    public QuoteReference(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "QuoteReference must not be empty.");

        var trimmed = value.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"QuoteReference exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
