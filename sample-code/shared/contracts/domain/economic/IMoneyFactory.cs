namespace Whycespace.Shared.Contracts.Domain.Economic;

/// <summary>
/// Engine-facing money creation contract.
/// Engines MUST NOT construct domain Money/Currency directly.
/// The runtime provides an implementation that delegates to domain value objects.
/// </summary>
public interface IMoneyFactory
{
    /// <summary>
    /// Creates a money value from amount and currency code.
    /// Returns domain Money — engines treat it as object and pass it through to aggregates.
    /// </summary>
    object CreateMoney(decimal amount, string currencyCode);

    /// <summary>
    /// Creates a currency from code.
    /// </summary>
    object CreateCurrency(string currencyCode);
}
