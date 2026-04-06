namespace Whycespace.Domain.IntelligenceSystem.Economic.Analysis;

public sealed class EconomicAnalysisService
{
    public Volume ComputeVolume(IReadOnlyList<decimal> transactionAmounts)
    {
        Guard.AgainstNull(transactionAmounts);
        var total = transactionAmounts.Sum();
        return new Volume(total);
    }

    public Velocity ComputeVelocity(int transactionCount, TimeSpan period)
    {
        if (period <= TimeSpan.Zero)
            return Velocity.Zero;

        var rate = transactionCount / (decimal)period.TotalSeconds;
        return new Velocity(rate);
    }
}
