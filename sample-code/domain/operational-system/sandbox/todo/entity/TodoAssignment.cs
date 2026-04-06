namespace Whycespace.Domain.OperationalSystem.Sandbox.Todo;

public sealed class TodoAssignment : Entity
{
    public Guid TodoId { get; private set; }
    public string AssignedTo { get; private set; } = null!;
    public DateTimeOffset AssignedAt { get; private set; }

    private TodoAssignment() { }

    public static TodoAssignment Create(Guid id, Guid todoId, string assignedTo, DateTimeOffset assignedAt)
    {
        Guard.AgainstDefault(id, nameof(id));
        Guard.AgainstEmpty(assignedTo, nameof(assignedTo));

        return new TodoAssignment
        {
            Id = id,
            TodoId = todoId,
            AssignedTo = assignedTo,
            AssignedAt = assignedAt
        };
    }
}
