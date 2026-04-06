namespace Whycespace.Domain.StructuralSystem.HumanCapital.Performance;

public sealed class PerformanceThresholdSpecification
{
    public bool IsSatisfiedBy(PerformanceRecordAggregate record) => record.Score.IsPassable;
}
