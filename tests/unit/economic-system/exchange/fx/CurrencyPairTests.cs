using Whycespace.Domain.EconomicSystem.Exchange.Fx;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Tests.Unit.EconomicSystem.Exchange.Fx;

/// <summary>
/// Phase 6 T6.2 — canonical ordering invariant on
/// <see cref="CurrencyPair"/>: Base MUST precede Quote ordinally. Prevents
/// duplicate representation (USD/EUR vs EUR/USD) which would break
/// deterministic rate lookup at ledger post time.
/// </summary>
public sealed class CurrencyPairTests
{
    [Theory]
    [InlineData("USD", "EUR")] // U > E ordinally
    [InlineData("GBP", "EUR")] // G > E
    [InlineData("USD", "USD")] // same code — equal, not strictly less
    [InlineData("ZZZ", "AAA")]
    public void Constructor_Rejects_WhenBaseDoesNotPrecedeQuoteOrdinally(string baseCode, string quoteCode)
    {
        var baseCurrency = new Currency(baseCode);
        var quoteCurrency = new Currency(quoteCode);

        Assert.Throws<ArgumentException>(() =>
            new CurrencyPair(baseCurrency, quoteCurrency));
    }

    [Theory]
    [InlineData("EUR", "USD")]
    [InlineData("AAA", "ZZZ")]
    [InlineData("ABC", "ABD")]
    public void Constructor_Accepts_WhenBaseStrictlyPrecedesQuote(string baseCode, string quoteCode)
    {
        var pair = new CurrencyPair(new Currency(baseCode), new Currency(quoteCode));

        Assert.Equal(baseCode, pair.BaseCurrency.Code);
        Assert.Equal(quoteCode, pair.QuoteCurrency.Code);
    }

    // 2026-04-20: Expected-exception type realigned with the actual
    // upstream throw. `new Currency("")` raises
    // DomainInvariantViolationException (via Guard.Against) BEFORE
    // CurrencyPair's own ArgumentException guards fire — so these
    // tests are asserting on the Currency-level invariant, not the
    // CurrencyPair-level one. DomainException is the parent class
    // (DomainInvariantViolationException inherits) so this assertion
    // covers both the current throw and any future subclass refinement.
    [Fact]
    public void Constructor_Rejects_WhenBaseCodeEmpty()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            new CurrencyPair(new Currency(""), new Currency("USD")));
    }

    [Fact]
    public void Constructor_Rejects_WhenQuoteCodeEmpty()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            new CurrencyPair(new Currency("EUR"), new Currency("")));
    }
}
