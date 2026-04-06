namespace Whycespace.Systems.Midstream.Wss.Workflows.Operational.Sandbox.Todo;

public sealed class CompleteTodoStep : ITodoWorkflowStep
{
    public Task<TodoStepResult> ExecuteAsync(
        TodoStepCommand command,
        CancellationToken ct = default)
    {
        if (command is not CompleteTodoStepCommand)
            return Task.FromResult(TodoStepResult.Fail(
                command.TodoId, nameof(CompleteTodoStep), "Invalid command type."));

        return Task.FromResult(TodoStepResult.Ok(
            command.TodoId, nameof(CompleteTodoStep)));
    }
}
