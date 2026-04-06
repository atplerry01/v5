using System.Text.Json;
using Whycespace.Runtime.Command;
using Whycespace.Shared.Contracts.Domain.Economic;
using Whycespace.Shared.Utils;

namespace Whycespace.Runtime.Context.Economic;

/// <summary>
/// Resolves IEconomicContext from CommandContext payload.
/// Extracts economic metadata (account, asset, amount, currency, transaction type)
/// from the command envelope payload for policy evaluation and chain anchoring.
/// </summary>
public sealed class EconomicContextResolver
{
    public IEconomicContext? Resolve(CommandContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var commandType = context.Envelope.CommandType;
        if (!IsEconomicCommand(commandType))
            return null;

        var payload = context.Envelope.Payload;
        var json = payload is JsonElement je ? je : ParsePayload(payload);
        if (json is null) return null;

        var accountId = GetString(json.Value, "AccountId") ?? GetString(json.Value, "WalletId") ?? "";
        var assetId = GetString(json.Value, "AssetId") ?? GetString(json.Value, "VaultId") ?? "";
        var amount = GetDecimal(json.Value, "Amount") ?? GetDecimal(json.Value, "TransactionAmount") ?? 0m;
        var currency = GetString(json.Value, "Currency") ?? GetString(json.Value, "CurrencyCode") ?? "USD";
        var txnType = ResolveTransactionType(commandType);

        return new ResolvedEconomicContext
        {
            AccountId = accountId,
            AssetId = assetId,
            Amount = AmountNormalizer.Normalize(amount, currency),
            Currency = currency.ToUpperInvariant().Trim(),
            TransactionType = txnType
        };
    }

    private static bool IsEconomicCommand(string commandType)
    {
        return commandType.StartsWith("economic.", StringComparison.OrdinalIgnoreCase)
            || commandType.StartsWith("capital.", StringComparison.OrdinalIgnoreCase)
            || commandType.StartsWith("ledger.", StringComparison.OrdinalIgnoreCase)
            || commandType.StartsWith("settlement.", StringComparison.OrdinalIgnoreCase)
            || commandType.StartsWith("revenue.", StringComparison.OrdinalIgnoreCase)
            || commandType.StartsWith("vault.", StringComparison.OrdinalIgnoreCase)
            || commandType.StartsWith("treasury.", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveTransactionType(string commandType)
    {
        var parts = commandType.Split('.');
        if (parts.Length < 2) return "unknown";

        var operation = parts[^1].ToLowerInvariant();
        return operation switch
        {
            "debit" or "withdraw" => "debit",
            "credit" or "deposit" => "credit",
            "transfer" or "move" => "transfer",
            "settle" or "settlement" => "settle",
            "allocate" or "reserve" => "allocate",
            "release" or "unlock" => "release",
            _ => operation
        };
    }

    private static JsonElement? ParsePayload(object? payload)
    {
        if (payload is null) return null;
        var s = JsonSerializer.Serialize(payload);
        return JsonDocument.Parse(s).RootElement;
    }

    private static string? GetString(JsonElement json, string prop)
        => json.TryGetProperty(prop, out var v) ? v.GetString() : null;

    private static decimal? GetDecimal(JsonElement json, string prop)
        => json.TryGetProperty(prop, out var v) && v.TryGetDecimal(out var d) ? d : null;
}

public sealed record ResolvedEconomicContext : IEconomicContext
{
    public required string AccountId { get; init; }
    public required string AssetId { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required string TransactionType { get; init; }
}
