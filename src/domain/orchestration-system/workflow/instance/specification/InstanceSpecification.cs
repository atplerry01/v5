using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Instance;

public sealed class CanStartSpecification : Specification<InstanceAggregate>
{
    public override bool IsSatisfiedBy(InstanceAggregate entity) =>
        entity.Status == InstanceStatus.Created;
}

public sealed class CanCompleteSpecification : Specification<InstanceAggregate>
{
    public override bool IsSatisfiedBy(InstanceAggregate entity) =>
        entity.Status == InstanceStatus.Running;
}

public sealed class CanFailSpecification : Specification<InstanceAggregate>
{
    public override bool IsSatisfiedBy(InstanceAggregate entity) =>
        entity.Status == InstanceStatus.Running;
}

public sealed class CanTerminateSpecification : Specification<InstanceAggregate>
{
    public override bool IsSatisfiedBy(InstanceAggregate entity) =>
        entity.Status != InstanceStatus.Completed &&
        entity.Status != InstanceStatus.Terminated;
}
