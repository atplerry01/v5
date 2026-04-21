namespace Whycespace.Domain.OperationalSystem.Sandbox.Todo;

public static class TodoErrors
{
    public const string EmptyIdentity = "Todo identity must be non-empty.";
    public const string TitleRequired = "Todo title is required.";
    public const string CannotUpdateCompleted = "Cannot update a completed todo.";
    public const string AlreadyCompleted = "Todo is already completed.";
}
