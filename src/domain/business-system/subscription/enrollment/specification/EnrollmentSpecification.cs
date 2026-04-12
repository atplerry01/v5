namespace Whycespace.Domain.BusinessSystem.Subscription.Enrollment;

public static class CanActivateSpecification
{
    public static bool IsSatisfiedBy(EnrollmentStatus status) => status == EnrollmentStatus.Pending;
}

public static class CanCancelSpecification
{
    public static bool IsSatisfiedBy(EnrollmentStatus status) =>
        status == EnrollmentStatus.Pending || status == EnrollmentStatus.Active;
}
