namespace Whycespace.Domain.BusinessSystem.Subscription.Enrollment;

public sealed class EnrollmentService
{
    public EnrollmentAggregate RequestEnrollment(EnrollmentId id, EnrollmentRequest request)
    {
        return EnrollmentAggregate.Request(id, request);
    }
}
