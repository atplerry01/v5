namespace Whycespace.Shared.Contracts.Policy;

/// <summary>
/// Unified policy evaluation context that ALL systems map into.
/// Ensures consistent policy evaluation regardless of entry point.
/// </summary>
public sealed record GlobalPolicyContext
{
    public required Guid ActorId { get; init; }
    public required string Action { get; init; }
    public required string Resource { get; init; }
    public required string Classification { get; init; }
    public required string Environment { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public Guid? PolicyId { get; init; }

    // Identity context
    public string? Role { get; init; }
    public string? TrustLevel { get; init; }

    // Economic context
    public decimal? Amount { get; init; }
    public string? Currency { get; init; }
    public string? AccountType { get; init; }

    // Workflow context
    public string? WorkflowId { get; init; }
    public string? WorkflowStep { get; init; }

    // Operational context
    public string? ClusterId { get; init; }
    public string? OperatorId { get; init; }

    public PolicyEvaluationInput ToEvaluationInput()
    {
        return new PolicyEvaluationInput(
            PolicyId, ActorId, Action, Classification, Environment, Timestamp);
    }
}
