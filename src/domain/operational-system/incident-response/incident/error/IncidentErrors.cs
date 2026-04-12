namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public static class IncidentErrors
{
    public static InvalidOperationException MissingId()
        => new("Incident ID must not be empty.");

    public static InvalidOperationException MissingDescriptor()
        => new("Incident descriptor must have a non-empty title and severity.");

    public static InvalidOperationException InvalidStateTransition(IncidentStatus status, string action)
        => new($"Cannot perform '{action}' when incident status is '{status}'.");
}
