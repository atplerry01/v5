using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed class IncidentCreationSpec : Specification<IncidentBaseAggregate>
{
    private readonly IncidentType _type;
    private readonly IncidentSeverity _severity;
    private readonly string _description;

    public IncidentCreationSpec(IncidentType type, IncidentSeverity severity, string description)
    {
        _type = type;
        _severity = severity;
        _description = description;
    }

    public override bool IsSatisfiedBy(IncidentBaseAggregate entity)
        => _type is not null
        && _severity is not null
        && !string.IsNullOrWhiteSpace(_description);
}
