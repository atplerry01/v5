namespace Whycespace.Domain.OrchestrationSystem.Workflow.Execution;

using Whycespace.Domain.SharedKernel;

public sealed class PolicyCheckpoint : ValueObject
{
    public string Name { get; }
    public bool IsBlocking { get; }

    private PolicyCheckpoint(string name, bool isBlocking)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        IsBlocking = isBlocking;
    }

    public static PolicyCheckpoint Blocking(string name) => new(name, isBlocking: true);
    public static PolicyCheckpoint Advisory(string name) => new(name, isBlocking: false);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return IsBlocking;
    }

    public override string ToString() => IsBlocking ? $"[blocking] {Name}" : $"[advisory] {Name}";
}
