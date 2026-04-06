using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.Incident.Response;

public sealed class IncidentResponseStatus : ValueObject
{
    public static readonly IncidentResponseStatus Detected = new("Detected");
    public static readonly IncidentResponseStatus Halted = new("Halted");
    public static readonly IncidentResponseStatus Investigating = new("Investigating");
    public static readonly IncidentResponseStatus Resolved = new("Resolved");
    public static readonly IncidentResponseStatus Closed = new("Closed");

    public string Value { get; }
    private IncidentResponseStatus(string value) => Value = value;
    public bool IsTerminal => this == Closed;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
