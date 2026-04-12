namespace Whycespace.Domain.BusinessSystem.Portfolio.Portfolio;

public readonly record struct PortfolioId
{
    public Guid Value { get; }

    public PortfolioId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PortfolioId value must not be empty.", nameof(value));

        Value = value;
    }
}
