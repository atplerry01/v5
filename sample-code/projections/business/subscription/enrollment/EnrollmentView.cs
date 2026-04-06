namespace Whycespace.Projections.Business.Subscription.Enrollment;

public sealed record EnrollmentView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
