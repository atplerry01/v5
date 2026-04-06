using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Runtime.Workflow;

public sealed class WorkflowRegistry : IWorkflowRegistry
{
    private readonly Dictionary<string, IReadOnlyList<Type>> _workflows = new();

    public void Register(string workflowName, IReadOnlyList<Type> stepTypes)
    {
        _workflows[workflowName] = stepTypes;
    }

    public IReadOnlyList<Type>? Resolve(string workflowName)
    {
        return _workflows.TryGetValue(workflowName, out var steps) ? steps : null;
    }
}
