namespace Whycespace.Shared.Contracts.Policy;

public interface IPolicyContext
{
    string PolicyId { get; }
    IReadOnlyDictionary<string, object> Facts { get; }
}
