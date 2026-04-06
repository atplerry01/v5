namespace Whycespace.Engines.T2E.Business.Integration.Job;

public class JobEngine
{
    private readonly JobPolicyAdapter _policy;

    public JobEngine(JobPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<JobResult> ExecuteAsync(JobCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new JobResult(true, "Executed");
    }
}
