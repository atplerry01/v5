namespace Whycespace.Domain.BusinessSystem.Portfolio.Holding;

public readonly record struct PortfolioReference
{
    public Guid Value { get; }

    public PortfolioReference(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PortfolioReference value must not be empty.", nameof(value));

        Value = value;
    }
}
