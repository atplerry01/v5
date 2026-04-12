namespace Whycespace.Domain.BusinessSystem.Inventory.Stock;

public static class StockErrors
{
    public static StockDomainException MissingId()
        => new("StockId is required and must not be empty.");

    public static StockDomainException MissingItemId()
        => new("StockItemId is required and must not be empty.");

    public static StockDomainException NegativeQuantity()
        => new("Stock quantity must not be negative.");

    public static StockDomainException InvalidStateTransition(StockStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static StockDomainException AlreadyTracked(StockId id)
        => new($"Stock '{id.Value}' is already being tracked.");

    public static StockDomainException AlreadyDepleted(StockId id)
        => new($"Stock '{id.Value}' is already depleted.");
}

public sealed class StockDomainException : Exception
{
    public StockDomainException(string message) : base(message) { }
}
