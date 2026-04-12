using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Definition;

public readonly record struct WorkflowBlueprint
{
    public string WorkflowName { get; }
    public int Version { get; }

    public WorkflowBlueprint(string workflowName, int version)
    {
        Guard.Against(string.IsNullOrWhiteSpace(workflowName), DefinitionErrors.MissingBlueprint);
        Guard.Against(version <= 0, DefinitionErrors.MissingBlueprint);
        WorkflowName = workflowName;
        Version = version;
    }
}
