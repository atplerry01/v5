namespace Whycespace.Domain.BusinessSystem.Execution.Charge;

public sealed class ChargeItem
{
    public Guid ItemId { get; }
    public string Description { get; }

    public ChargeItem(Guid itemId, string description)
    {
        if (itemId == Guid.Empty)
            throw new ArgumentException("ItemId must not be empty.", nameof(itemId));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description must not be empty.", nameof(description));

        ItemId = itemId;
        Description = description;
    }
}
