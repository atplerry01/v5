using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Performance;

public sealed class PerformanceRecordAggregate : AggregateRoot
{
    public Score Score { get; private set; } = null!;
    public EvaluationPeriod EvaluationPeriod { get; private set; } = null!;
    public PerformanceMetric PerformanceMetric { get; private set; } = null!;

    public static PerformanceRecordAggregate Create(Guid recordId, double score, DateTimeOffset periodStart, DateTimeOffset periodEnd, PerformanceMetric metric)
    {
        if (periodEnd <= periodStart)
            throw new DomainException(PerformanceErrors.InvalidData, "Evaluation period end must be after start.");

        var record = new PerformanceRecordAggregate();
        record.Apply(new PerformanceRecordedEvent(recordId, score, periodStart, periodEnd));
        record.PerformanceMetric = metric;
        return record;
    }

    public void Update(double newScore)
    {
        Apply(new PerformanceUpdatedEvent(Id, newScore));
    }

    private void Apply(PerformanceRecordedEvent e)
    {
        Id = e.RecordId;
        Score = new Score(e.Score);
        EvaluationPeriod = new EvaluationPeriod(e.PeriodStart, e.PeriodEnd);
        RaiseDomainEvent(e);
    }

    private void Apply(PerformanceUpdatedEvent e)
    {
        Score = new Score(e.NewScore);
        RaiseDomainEvent(e);
    }
}
