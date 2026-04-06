namespace Whycespace.Shared.Contracts.Policy;

public interface IPolicyDecision
{
    bool IsAllowed { get; }
    string? Reason { get; }
}
