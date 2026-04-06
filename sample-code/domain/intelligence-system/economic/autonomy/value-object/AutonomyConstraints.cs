namespace Whycespace.Domain.IntelligenceSystem.Economic.Autonomy;

public sealed class AutonomyConstraints
{
    public decimal MaxCost { get; }
    public IReadOnlyCollection<Guid> AllowedEntities { get; }

    public AutonomyConstraints(decimal maxCost, IEnumerable<Guid> allowed)
    {
        MaxCost = maxCost;
        AllowedEntities = allowed.ToList().AsReadOnly();
    }
}
