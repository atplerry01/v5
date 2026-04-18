namespace Whycespace.Shared.Contracts.Economic.Exchange.Rate;

/// <summary>
/// Phase 4 T4.4 — read-side port that resolves the active exchange rate for
/// a (base, quote) currency pair so the transaction control plane can lock
/// a deterministic rate before ledger posting. Returns <c>null</c> when no
/// active rate exists for the pair; FXLockStep treats that as a hard-fail
/// because cross-currency transactions cannot deterministically post
/// without a bound rate.
///
/// CQRS-clean: implementations consult <c>exchange_rate_read_model</c>
/// only — never the write-side aggregate.
/// </summary>
public interface IExchangeRateResolver
{
    Task<ExchangeRateResolution?> ResolveAsync(
        string baseCurrency,
        string quoteCurrency,
        DateTimeOffset asOf,
        CancellationToken cancellationToken = default);
}

public sealed record ExchangeRateResolution(
    Guid RateId,
    string BaseCurrency,
    string QuoteCurrency,
    decimal Rate,
    DateTimeOffset EffectiveAt,
    int VersionNumber);
