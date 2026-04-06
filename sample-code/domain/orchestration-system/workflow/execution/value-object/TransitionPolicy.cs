namespace Whycespace.Domain.OrchestrationSystem.Workflow.Execution;

using Whycespace.Domain.SharedKernel;

public sealed class TransitionPolicy : ValueObject
{
    public Guid PolicyRuleId { get; }
    public string Description { get; }

    private TransitionPolicy(Guid policyRuleId, string description)
    {
        if (policyRuleId == Guid.Empty)
            throw new ArgumentException("PolicyRuleId cannot be empty.", nameof(policyRuleId));
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        PolicyRuleId = policyRuleId;
        Description = description;
    }

    public static TransitionPolicy For(Guid policyRuleId, string description) =>
        new(policyRuleId, description);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return PolicyRuleId;
    }

    public override string ToString() => $"Policy:{PolicyRuleId} ({Description})";
}
