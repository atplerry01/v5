namespace Whycespace.Domain.StructuralSystem.HumanCapital.Assignment;

public sealed class AssignmentStatus
{
    public Guid Id { get; }
    public string StatusName { get; }

    private AssignmentStatus(Guid id, string statusName)
    {
        Id = id;
        StatusName = statusName;
    }

    public static readonly AssignmentStatus Created = new(Guid.Empty, "created");
    public static readonly AssignmentStatus Started = new(Guid.Empty, "started");
    public static readonly AssignmentStatus Completed = new(Guid.Empty, "completed");
    public static readonly AssignmentStatus Failed = new(Guid.Empty, "failed");

    public bool IsTerminal => this == Completed || this == Failed;
}
