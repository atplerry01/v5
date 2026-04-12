namespace Whycespace.Domain.BusinessSystem.Subscription.Enrollment;

public sealed record EnrollmentRequestedEvent(EnrollmentId EnrollmentId, EnrollmentRequest Request);

public sealed record EnrollmentActivatedEvent(EnrollmentId EnrollmentId);

public sealed record EnrollmentCancelledEvent(EnrollmentId EnrollmentId);
