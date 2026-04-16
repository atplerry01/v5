namespace Whycespace.Domain.ContentSystem.Monetization.Payout;

public readonly record struct ContentPayoutId(Guid Value)
{
    public static ContentPayoutId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
