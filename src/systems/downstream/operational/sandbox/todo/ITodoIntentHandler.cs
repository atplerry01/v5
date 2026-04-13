namespace Whycespace.Systems.Downstream.Operational.Sandbox.Todo;

public interface ITodoIntentHandler
{
    Task<TodoSystemResult> HandleCreateAsync(string title, string userId);
}
