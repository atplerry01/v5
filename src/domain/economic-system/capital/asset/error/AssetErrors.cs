using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Asset;

public static class AssetErrors
{
    public static DomainException AssetAlreadyDisposed()
        => new("Asset has already been disposed.");

    public static DomainException InvalidAmount()
        => new("Amount must be greater than zero.");

    public static DomainException InvalidCurrencyCode()
        => new("Currency code is invalid.");

    public static DomainException CurrencyMismatch(string expected, string actual)
        => new($"Currency mismatch: expected '{expected}', actual '{actual}'.");

    public static DomainException CannotValueDisposedAsset()
        => new("Cannot revalue a disposed asset.");

    public static DomainInvariantViolationException NegativeAssetValue()
        => new("Asset value must not be negative.");

    public static DomainException InvalidOwnerId()
        => new("Owner ID cannot be empty.");
}
