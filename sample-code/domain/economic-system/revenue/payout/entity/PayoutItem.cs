namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public sealed class PayoutItem
{
    public Guid ItemId { get; }
    public string Description { get; }
    public decimal Amount { get; }
    public string Category { get; }

    public PayoutItem(Guid itemId, string description, decimal amount, string category)
    {
        if (itemId == Guid.Empty)
            throw new PayoutException("Payout item id is required.");

        if (string.IsNullOrWhiteSpace(description))
            throw new PayoutException("Payout item description is required.");

        if (amount <= 0)
            throw new PayoutException("Payout item amount must be positive.");

        if (string.IsNullOrWhiteSpace(category))
            throw new PayoutException("Payout item category is required.");

        ItemId = itemId;
        Description = description;
        Amount = amount;
        Category = category;
    }
}
