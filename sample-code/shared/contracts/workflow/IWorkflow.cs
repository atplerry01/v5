namespace Whycespace.Shared.Contracts.Workflow;

public interface IWorkflow
{
    string WorkflowId { get; }
    string Name { get; }
    IReadOnlyList<IWorkflowStep> Steps { get; }
}
