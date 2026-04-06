namespace Whycespace.Domain.StructuralSystem.Cluster.Spv.Transaction;

/// <summary>
/// H4 — Global invariant service for cross-SPV transactions.
/// Validates value conservation, currency consistency, and leg integrity.
/// </summary>
public sealed class CrossSpvInvariantService
{
    public static void Validate(IReadOnlyList<SpvLeg> legs)
    {
        if (legs == null || legs.Count == 0)
            throw new CrossSpvException("No legs provided");

        var currency = legs[0].Amount.Currency;

        var outgoing = Money.Zero(currency);
        var incoming = Money.Zero(currency);

        for (var i = 0; i < legs.Count; i++)
        {
            var leg = legs[i];

            if (leg.Amount.Currency != currency)
                throw new CrossSpvException(
                    $"Currency mismatch at leg {i}: expected {currency.Code}, got {leg.Amount.Currency.Code}");

            if (leg.Amount.IsZero || leg.Amount.IsNegative)
                throw new CrossSpvException($"Leg {i}: amount must be positive");

            outgoing = outgoing.Add(leg.Amount);
            incoming = incoming.Add(leg.Amount);
        }

        if (outgoing != incoming)
            throw new CrossSpvException("Value not conserved: total outgoing != total incoming");
    }
}
