namespace Whycespace.Domain.OperationalSystem.Sandbox.Todo;

public sealed class TodoItem : AggregateRoot
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? AssignedTo { get; private set; }
    public int Priority { get; private set; }
    public bool IsCompleted { get; private set; }

    public TodoItem() { }

    public static TodoItem Create(Guid id, string title, string description, string? assignedTo, int priority)
    {
        Guard.AgainstEmpty(title, nameof(title));

        var item = new TodoItem
        {
            Id = id,
            Title = title,
            Description = description,
            AssignedTo = assignedTo,
            Priority = priority
        };

        item.RaiseDomainEvent(new TodoCreatedEvent(id, title, description, assignedTo, priority));
        return item;
    }

    public void Complete()
    {
        EnsureInvariant(!IsCompleted, TodoErrors.AlreadyCompleted, "Todo item is already completed.");
        IsCompleted = true;
        RaiseDomainEvent(new TodoCompletedEvent(Id));
    }
}
