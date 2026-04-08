namespace Whyce.Shared.Contracts.Application.Todo;

/// <summary>
/// Canonical workflow-name constants for the Todo lifecycle.
///
/// Relocated from <c>src/systems/midstream/wss/workflows/todo/TodoLifecycleWorkflow.cs</c>
/// under Phase 1.5 §5.1.2 BPV-D02 (Option D02-A) so that downstream and host
/// consumers can reference the workflow name without crossing the
/// systems/downstream → systems/midstream tier boundary.
/// </summary>
public static class TodoLifecycleWorkflowNames
{
    public const string Create = "todo.lifecycle.create";
}
