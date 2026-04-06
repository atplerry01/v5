namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

using Whycespace.Domain.SharedKernel;

public sealed class ProposalStatus : ValueObject
{
    public string Value { get; }
    private ProposalStatus(string value) => Value = value;

    public static readonly ProposalStatus Draft = new("draft");
    public static readonly ProposalStatus Active = new("active");
    public static readonly ProposalStatus Approved = new("approved");
    public static readonly ProposalStatus Rejected = new("rejected");
    public static readonly ProposalStatus Executed = new("executed");

    public static ProposalStatus From(string value) => new(value.ToLowerInvariant());

    protected override IEnumerable<object> GetEqualityComponents() { yield return Value; }
    public override string ToString() => Value;
}
