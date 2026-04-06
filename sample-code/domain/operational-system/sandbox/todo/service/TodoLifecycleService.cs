namespace Whycespace.Domain.OperationalSystem.Sandbox.Todo;

public sealed class TodoLifecycleService
{
    public bool CanComplete(TodoItem todo)
    {
        return !todo.IsCompleted;
    }
}
