using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.RateCard;

public readonly record struct RateEntry
{
    public const int CodeMaxLength = 64;
    public const int UnitMaxLength = 32;

    public string Code { get; }
    public decimal Amount { get; }
    public string Unit { get; }

    public RateEntry(string code, decimal amount, string unit)
    {
        Guard.Against(string.IsNullOrWhiteSpace(code), "RateEntry code must not be empty.");
        Guard.Against(code!.Trim().Length > CodeMaxLength, $"RateEntry code exceeds {CodeMaxLength} characters.");
        Guard.Against(amount < 0m, "RateEntry amount must be non-negative.");
        Guard.Against(string.IsNullOrWhiteSpace(unit), "RateEntry unit must not be empty.");
        Guard.Against(unit!.Trim().Length > UnitMaxLength, $"RateEntry unit exceeds {UnitMaxLength} characters.");

        Code = code.Trim();
        Amount = amount;
        Unit = unit.Trim();
    }
}
