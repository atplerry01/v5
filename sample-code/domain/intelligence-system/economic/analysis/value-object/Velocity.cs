namespace Whycespace.Domain.IntelligenceSystem.Economic.Analysis;

public sealed record Velocity
{
    public decimal TransactionsPerSecond { get; }

    public Velocity(decimal transactionsPerSecond)
    {
        if (transactionsPerSecond < 0)
            throw new ArgumentException("Velocity cannot be negative.", nameof(transactionsPerSecond));
        TransactionsPerSecond = transactionsPerSecond;
    }

    public static Velocity Zero => new(0);
}
