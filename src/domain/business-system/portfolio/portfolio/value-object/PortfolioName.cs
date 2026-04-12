namespace Whycespace.Domain.BusinessSystem.Portfolio.Portfolio;

public readonly record struct PortfolioName
{
    public string Value { get; }

    public PortfolioName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("PortfolioName must not be empty.", nameof(value));

        Value = value;
    }
}
