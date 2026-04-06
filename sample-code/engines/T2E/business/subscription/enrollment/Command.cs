namespace Whycespace.Engines.T2E.Business.Subscription.Enrollment;

public record EnrollmentCommand(
    string Action,
    string EntityId,
    object Payload
);
