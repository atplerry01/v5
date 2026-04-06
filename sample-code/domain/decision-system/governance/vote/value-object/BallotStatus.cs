namespace Whycespace.Domain.DecisionSystem.Governance.Vote;

public sealed record BallotStatus(string Value)
{
    public static readonly BallotStatus Open = new("Open");
    public static readonly BallotStatus Closed = new("Closed");

    public override string ToString() => Value;
}
