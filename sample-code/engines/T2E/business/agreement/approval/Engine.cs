namespace Whycespace.Engines.T2E.Business.Agreement.Approval;

public class ApprovalEngine
{
    private readonly ApprovalPolicyAdapter _policy;

    public ApprovalEngine(ApprovalPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ApprovalResult> ExecuteAsync(ApprovalCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ApprovalResult(true, "Executed");
    }
}
