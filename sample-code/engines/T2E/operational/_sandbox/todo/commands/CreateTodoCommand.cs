namespace Whycespace.Engines.T2E.Operational._Sandbox.Todo;

public sealed record CreateTodoCommand(
    Guid TodoId,
    string Title,
    string Description,
    string AssignedTo,
    int Priority
) : TodoCommand;
