namespace Whycespace.Shared.Contracts.Workflow;

public interface IWorkflowStep
{
    string StepId { get; }
    string Name { get; }
    Task<bool> ExecuteAsync(CancellationToken cancellationToken);
}
