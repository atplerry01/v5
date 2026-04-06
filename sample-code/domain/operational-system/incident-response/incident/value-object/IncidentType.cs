namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed record IncidentType(string Value)
{
    public static readonly IncidentType System = new("system");
    public static readonly IncidentType Business = new("business");
    public static readonly IncidentType Policy = new("policy");
    public static readonly IncidentType Compliance = new("compliance");

    private static readonly HashSet<string> ValidValues = [System.Value, Business.Value, Policy.Value, Compliance.Value];

    public static IncidentType From(string value)
    {
        if (!ValidValues.Contains(value))
            throw new ArgumentException($"Invalid incident type: '{value}'.");

        return new IncidentType(value);
    }
}
