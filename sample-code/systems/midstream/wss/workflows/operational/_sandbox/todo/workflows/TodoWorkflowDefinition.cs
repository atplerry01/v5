namespace Whycespace.Systems.Midstream.Wss.Workflows.Operational.Sandbox.Todo;

/// <summary>
/// Defines the todo lifecycle workflow — step sequence for sandbox todo domain.
/// Systems layer: composition only, no execution.
/// </summary>
public static class TodoWorkflowDefinition
{
    public const string WorkflowId = "TODO_LIFECYCLE_V1";

    public static readonly IReadOnlyList<string> Steps =
    [
        "CreateTodo",
        "CompleteTodo"
    ];
}
