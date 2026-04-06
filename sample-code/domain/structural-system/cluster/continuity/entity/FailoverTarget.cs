using DomainEntity = Whycespace.Domain.SharedKernel.Primitives.Kernel.Entity;

namespace Whycespace.Domain.StructuralSystem.Cluster.Continuity;

public sealed class FailoverTarget : DomainEntity
{
    public Guid TargetId { get; private set; }
    public string TargetType { get; private set; } = string.Empty;
    public int Priority { get; private set; }
    public HealthThreshold HealthThreshold { get; private set; } = null!;

    private FailoverTarget() { }

    public static FailoverTarget Create(
        Guid id,
        Guid targetId,
        string targetType,
        int priority,
        HealthThreshold healthThreshold)
    {
        return new FailoverTarget
        {
            Id = id,
            TargetId = targetId,
            TargetType = targetType,
            Priority = priority,
            HealthThreshold = healthThreshold
        };
    }
}
