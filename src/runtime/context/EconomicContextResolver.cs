namespace Whycespace.Runtime.Context;

/// <summary>
/// Resolves economic context from command payload.
/// Extracts economic metadata (currency, amount, ledger references)
/// for commands that involve economic operations.
/// </summary>
public sealed class EconomicContextResolver
{
    /// <summary>
    /// Attempts to resolve economic context from the command payload.
    /// Returns null if the command does not contain economic metadata.
    /// </summary>
    public EconomicContext? Resolve(object command)
    {
        // Economic context is extracted from command properties by convention.
        // Commands with economic impact declare: Currency, Amount, LedgerId.
        var type = command.GetType();
        var currencyProp = type.GetProperty("Currency");
        var amountProp = type.GetProperty("Amount");

        if (currencyProp is null || amountProp is null)
            return null;

        var currency = currencyProp.GetValue(command) as string;
        var amount = amountProp.GetValue(command) as decimal? ?? 0m;
        var ledgerIdProp = type.GetProperty("LedgerId");
        var ledgerId = ledgerIdProp?.GetValue(command) as string;

        return new EconomicContext
        {
            Currency = currency ?? "USD",
            Amount = amount,
            LedgerId = ledgerId
        };
    }
}

public sealed record EconomicContext
{
    public required string Currency { get; init; }
    public required decimal Amount { get; init; }
    public string? LedgerId { get; init; }
}
