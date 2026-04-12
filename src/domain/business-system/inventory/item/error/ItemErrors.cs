namespace Whycespace.Domain.BusinessSystem.Inventory.Item;

public static class ItemErrors
{
    public static ItemDomainException MissingId()
        => new("ItemId is required and must not be empty.");

    public static ItemDomainException MissingTypeId()
        => new("ItemTypeId is required and must not be empty.");

    public static ItemDomainException InvalidStateTransition(ItemStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ItemDomainException AlreadyDiscontinued(ItemId id)
        => new($"Item '{id.Value}' has already been discontinued.");
}

public sealed class ItemDomainException : Exception
{
    public ItemDomainException(string message) : base(message) { }
}
