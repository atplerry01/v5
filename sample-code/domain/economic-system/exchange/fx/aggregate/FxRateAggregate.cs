using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Exchange.Fx;

/// <summary>
/// FX rate authority. Manages currency pair exchange rates.
/// Enforces rate validity windows and prevents stale rate usage.
/// All rates are immutable once published — updates create new entries.
/// </summary>
public sealed class FxRateAggregate : AggregateRoot
{
    public CurrencyPair Pair { get; private set; } = null!;
    public decimal Rate { get; private set; }
    public DateTimeOffset EffectiveFrom { get; private set; }
    public DateTimeOffset? EffectiveUntil { get; private set; }
    public FxRateStatus Status { get; private set; } = FxRateStatus.Pending;
    public string Source { get; private set; } = string.Empty;

    public static FxRateAggregate Publish(
        Guid id, CurrencyPair pair, decimal rate,
        DateTimeOffset effectiveFrom, string source)
    {
        var agg = new FxRateAggregate
        {
            Id = id,
            Pair = pair,
            Rate = rate,
            EffectiveFrom = effectiveFrom,
            Source = source,
            Status = FxRateStatus.Active
        };
        agg.RaiseDomainEvent(new FxRatePublishedEvent(id, pair.BaseCurrency, pair.QuoteCurrency, rate, effectiveFrom, source));
        return agg;
    }

    public void Supersede(DateTimeOffset effectiveUntil)
    {
        EnsureNotTerminal(Status, s => s.IsTerminal, "Supersede");
        EnsureInvariant(effectiveUntil > EffectiveFrom, "ValidWindow", "EffectiveUntil must be after EffectiveFrom.");
        EffectiveUntil = effectiveUntil;
        Status = FxRateStatus.Superseded;
        RaiseDomainEvent(new FxRateSupersededEvent(Id, effectiveUntil));
    }

    public void Invalidate(string reason)
    {
        EnsureNotTerminal(Status, s => s.IsTerminal, "Invalidate");
        Status = FxRateStatus.Invalid;
        RaiseDomainEvent(new FxRateInvalidatedEvent(Id, reason));
    }

    /// <summary>
    /// Converts an amount from base currency to quote currency at this rate.
    /// </summary>
    public decimal Convert(decimal amount) => amount * Rate;

    /// <summary>
    /// Converts an amount from quote currency to base currency (inverse).
    /// </summary>
    public decimal ConvertInverse(decimal amount) =>
        Rate == 0m ? throw new InvalidOperationException("Cannot invert zero rate.") : amount / Rate;
}
