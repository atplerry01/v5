using DomainEntity = Whycespace.Domain.SharedKernel.Primitives.Kernel.Entity;

namespace Whycespace.Domain.StructuralSystem.Cluster.Continuity;

public sealed class RecoveryStep : DomainEntity
{
    public Guid StepId { get; private set; }
    public int Order { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public int TimeoutSeconds { get; private set; }

    private RecoveryStep() { }

    public static RecoveryStep Create(
        Guid id,
        Guid stepId,
        int order,
        string description,
        int timeoutSeconds)
    {
        return new RecoveryStep
        {
            Id = id,
            StepId = stepId,
            Order = order,
            Description = description,
            TimeoutSeconds = timeoutSeconds
        };
    }
}
