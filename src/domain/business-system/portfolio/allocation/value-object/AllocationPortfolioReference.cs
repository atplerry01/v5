namespace Whycespace.Domain.BusinessSystem.Portfolio.Allocation;

public readonly record struct AllocationPortfolioReference
{
    public Guid Value { get; }

    public AllocationPortfolioReference(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AllocationPortfolioReference value must not be empty.", nameof(value));

        Value = value;
    }
}
