namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed record IncidentReference(string Domain, Guid EntityId)
{
    public static IncidentReference ForRide(Guid rideId) => new("mobility.taxi.ride", rideId);
    public static IncidentReference ForTenancy(Guid tenancyId) => new("property.propertyletting.tenancy", tenancyId);
    public static IncidentReference ForPayment(Guid paymentId) => new("economic.settlement", paymentId);
    public static IncidentReference ForWorkforce(Guid assignmentId) => new("operational.workforce", assignmentId);

    public static readonly IncidentReference None = new(string.Empty, Guid.Empty);

    public bool IsEmpty => string.IsNullOrEmpty(Domain) || EntityId == Guid.Empty;

    public override string ToString() => IsEmpty ? "none" : $"{Domain}:{EntityId}";
}
