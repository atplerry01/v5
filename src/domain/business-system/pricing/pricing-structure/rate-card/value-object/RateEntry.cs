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
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("RateEntry code must not be empty.", nameof(code));

        if (code.Trim().Length > CodeMaxLength)
            throw new ArgumentException($"RateEntry code exceeds {CodeMaxLength} characters.", nameof(code));

        if (amount < 0m)
            throw new ArgumentException("RateEntry amount must be non-negative.", nameof(amount));

        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("RateEntry unit must not be empty.", nameof(unit));

        if (unit.Trim().Length > UnitMaxLength)
            throw new ArgumentException($"RateEntry unit exceeds {UnitMaxLength} characters.", nameof(unit));

        Code = code.Trim();
        Amount = amount;
        Unit = unit.Trim();
    }
}
