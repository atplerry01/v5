namespace Whycespace.Domain.OperationalSystem.Sandbox.Todo;

public sealed class TodoCompletionSpec : Specification<TodoItem>
{
    public override bool IsSatisfiedBy(TodoItem entity)
    {
        return !entity.IsCompleted;
    }
}
