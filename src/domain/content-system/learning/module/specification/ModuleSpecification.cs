using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Module;

public sealed class ModuleSpecification : Specification<ModuleStatus>
{
    public override bool IsSatisfiedBy(ModuleStatus entity) =>
        entity == ModuleStatus.Draft || entity == ModuleStatus.Published;

    public void EnsureMutable(ModuleStatus status)
    {
        if (status == ModuleStatus.Archived) throw ModuleErrors.CannotMutateArchived();
    }
}
