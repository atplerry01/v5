namespace Whycespace.Domain.DecisionSystem.Governance.Dispute;

public readonly record struct DisputeOutcome(string Verdict, string? Justification = null)
{
    public static readonly DisputeOutcome None = new(string.Empty);

    public bool HasVerdict => !string.IsNullOrWhiteSpace(Verdict);
}
