namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public readonly record struct IncidentId
{
    public Guid Value { get; }

    public IncidentId(Guid value)
    {
        if (value == Guid.Empty)
            throw IncidentErrors.MissingId();

        Value = value;
    }
}

public enum IncidentStatus
{
    Reported,
    Investigating,
    Resolved,
    Closed
}

public readonly record struct IncidentDescriptor
{
    public string Title { get; }
    public string Severity { get; }

    public IncidentDescriptor(string title, string severity)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw IncidentErrors.MissingDescriptor();

        if (string.IsNullOrWhiteSpace(severity))
            throw IncidentErrors.MissingDescriptor();

        Title = title;
        Severity = severity;
    }
}
