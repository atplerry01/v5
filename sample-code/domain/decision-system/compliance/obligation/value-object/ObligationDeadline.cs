namespace Whycespace.Domain.DecisionSystem.Compliance.Obligation;

public sealed record ObligationDeadline(DateTimeOffset DueDate, int GracePeriodDays = 0)
{
    public DateTimeOffset EffectiveDeadline => DueDate.AddDays(GracePeriodDays);

    public bool IsOverdue(DateTimeOffset asOf) => asOf > EffectiveDeadline;

    public int DaysRemaining(DateTimeOffset asOf)
    {
        var remaining = (EffectiveDeadline - asOf).Days;
        return remaining < 0 ? 0 : remaining;
    }
}
