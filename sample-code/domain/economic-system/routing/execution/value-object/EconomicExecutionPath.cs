namespace Whycespace.Domain.EconomicSystem.Routing.Execution;

public sealed class EconomicExecutionPath
{
    public Guid SourceEntityId { get; }
    public Guid TargetEntityId { get; }
    public IReadOnlyCollection<Guid> Path { get; }

    public EconomicExecutionPath(Guid source, Guid target, IEnumerable<Guid> path)
    {
        SourceEntityId = source;
        TargetEntityId = target;
        Path = path.ToList().AsReadOnly();
    }

    public bool IsEmpty() => Path.Count == 0;
}
