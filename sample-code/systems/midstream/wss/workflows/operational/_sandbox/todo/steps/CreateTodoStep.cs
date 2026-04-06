namespace Whycespace.Systems.Midstream.Wss.Workflows.Operational.Sandbox.Todo;

public sealed class CreateTodoStep : ITodoWorkflowStep
{
    public Task<TodoStepResult> ExecuteAsync(
        TodoStepCommand command,
        CancellationToken ct = default)
    {
        if (command is not CreateTodoStepCommand)
            return Task.FromResult(TodoStepResult.Fail(
                command.TodoId, nameof(CreateTodoStep), "Invalid command type."));

        return Task.FromResult(TodoStepResult.Ok(
            command.TodoId, nameof(CreateTodoStep)));
    }
}
