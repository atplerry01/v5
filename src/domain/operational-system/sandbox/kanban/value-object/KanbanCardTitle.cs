namespace Whycespace.Domain.OperationalSystem.Sandbox.Kanban;

/// Typed structural title for a kanban card. Non-evolving, non-versioned —
/// captured once at creation time and replaced via revise commands. NOT
/// content (does not externalise to `DocumentAggregate`).
public readonly record struct KanbanCardTitle
{
    public string Value { get; }

    public KanbanCardTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("KanbanCardTitle must not be empty.", nameof(value));

        if (value.Length > 256)
            throw new ArgumentException("KanbanCardTitle must not exceed 256 characters.", nameof(value));

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
