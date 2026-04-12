using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Definition;

public readonly record struct DefinitionId
{
    public Guid Value { get; }

    public DefinitionId(Guid value)
    {
        Guard.Against(value == Guid.Empty, DefinitionErrors.MissingId);
        Value = value;
    }
}
