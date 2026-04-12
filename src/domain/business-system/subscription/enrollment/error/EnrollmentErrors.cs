namespace Whycespace.Domain.BusinessSystem.Subscription.Enrollment;

public static class EnrollmentErrors
{
    public static InvalidOperationException MissingId()
        => new("Enrollment ID must not be empty.");

    public static InvalidOperationException MissingRequest()
        => new("Enrollment request must have non-empty AccountReference and PlanReference.");

    public static InvalidOperationException InvalidStateTransition(EnrollmentStatus status, string action)
        => new($"Cannot perform '{action}' when enrollment status is '{status}'.");
}
