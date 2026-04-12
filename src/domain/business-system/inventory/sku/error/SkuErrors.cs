namespace Whycespace.Domain.BusinessSystem.Inventory.Sku;

public static class SkuErrors
{
    public static SkuDomainException MissingId()
        => new("SkuId is required and must not be empty.");

    public static SkuDomainException MissingCode()
        => new("SkuCode is required and must not be empty.");

    public static SkuDomainException AlreadyDiscontinued(SkuId id)
        => new($"SKU '{id.Value}' has already been discontinued.");

    public static SkuDomainException InvalidStateTransition(SkuStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class SkuDomainException : Exception
{
    public SkuDomainException(string message) : base(message) { }
}
