namespace Whycespace.Domain.EconomicSystem.Transaction.Charge;

/// <summary>
/// Individual line item within a charge (e.g., base fee + tax + surcharge).
/// Immutable once created — line items are not modified after attachment.
/// </summary>
public sealed class ChargeLineItem
{
    public Guid LineItemId { get; }
    public string Description { get; }
    public decimal Amount { get; }
    public string Category { get; }

    private ChargeLineItem(Guid lineItemId, string description, decimal amount, string category)
    {
        LineItemId = lineItemId;
        Description = description;
        Amount = amount;
        Category = category;
    }

    public static ChargeLineItem Create(Guid lineItemId, string description, decimal amount, string category)
    {
        if (lineItemId == Guid.Empty)
            throw new DomainException("CHARGE.LINE_ITEM_ID_REQUIRED", "Line item id is required.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("CHARGE.LINE_ITEM_DESCRIPTION_REQUIRED", "Line item description is required.");

        if (amount <= 0)
            throw new DomainException("CHARGE.LINE_ITEM_INVALID_AMOUNT", $"Line item amount must be greater than zero. Got: {amount}");

        if (string.IsNullOrWhiteSpace(category))
            throw new DomainException("CHARGE.LINE_ITEM_CATEGORY_REQUIRED", "Line item category is required.");

        return new ChargeLineItem(lineItemId, description, amount, category);
    }
}
