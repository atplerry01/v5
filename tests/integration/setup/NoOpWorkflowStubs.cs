using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Runtime;

namespace Whycespace.Tests.Integration.Setup;

/// <summary>
/// Stubs for the workflow surface required by RuntimeCommandDispatcher's constructor.
/// Todo does not use the workflow path, so these throw if invoked. Their presence
/// satisfies the constructor signature without coupling tests to workflow internals.
/// </summary>
public sealed class NoOpWorkflowEngine : IWorkflowEngine
{
    public Task<WorkflowExecutionResult> ExecuteAsync(WorkflowDefinition definition, WorkflowExecutionContext context)
        => throw new InvalidOperationException("NoOpWorkflowEngine should not be invoked in non-workflow tests.");
}

public sealed class NoOpWorkflowRegistry : IWorkflowRegistry
{
    public void Register(string workflowName, IReadOnlyList<Type> stepTypes) { }
    public IReadOnlyList<Type>? Resolve(string workflowName) => null;
}

public sealed class InMemoryWorkflowStateRepository : IWorkflowStateRepository
{
    private readonly Dictionary<string, WorkflowStateRecord> _store = new();
    public Task SaveAsync(WorkflowStateRecord state) { _store[state.WorkflowId] = state; return Task.CompletedTask; }
    public Task<WorkflowStateRecord?> GetAsync(string workflowId) => Task.FromResult(_store.GetValueOrDefault(workflowId));
    public Task UpdateAsync(WorkflowStateRecord state) { _store[state.WorkflowId] = state; return Task.CompletedTask; }
}
