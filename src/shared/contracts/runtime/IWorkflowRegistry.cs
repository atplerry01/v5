namespace Whycespace.Shared.Contracts.Runtime;

public interface IWorkflowRegistry
{
    void Register(string workflowName, IReadOnlyList<Type> stepTypes);
    IReadOnlyList<Type>? Resolve(string workflowName);
}
