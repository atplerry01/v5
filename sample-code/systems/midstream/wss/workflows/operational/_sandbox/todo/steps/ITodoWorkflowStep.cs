namespace Whycespace.Systems.Midstream.Wss.Workflows.Operational.Sandbox.Todo;

public interface ITodoWorkflowStep
{
    Task<TodoStepResult> ExecuteAsync(
        TodoStepCommand command,
        CancellationToken ct = default);
}

public abstract record TodoStepCommand(Guid TodoId);

public sealed record CreateTodoStepCommand(
    Guid TodoId,
    string Title,
    string Description
) : TodoStepCommand(TodoId);

public sealed record CompleteTodoStepCommand(
    Guid TodoId
) : TodoStepCommand(TodoId);

public sealed record TodoStepResult(
    Guid TodoId,
    string StepName,
    bool Success,
    string? FailureReason = null)
{
    public static TodoStepResult Ok(Guid todoId, string stepName)
        => new(todoId, stepName, true);

    public static TodoStepResult Fail(Guid todoId, string stepName, string reason)
        => new(todoId, stepName, false, reason);
}
