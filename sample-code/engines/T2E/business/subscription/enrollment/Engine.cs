namespace Whycespace.Engines.T2E.Business.Subscription.Enrollment;

public class EnrollmentEngine
{
    private readonly EnrollmentPolicyAdapter _policy;

    public EnrollmentEngine(EnrollmentPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<EnrollmentResult> ExecuteAsync(EnrollmentCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new EnrollmentResult(true, "Executed");
    }
}
