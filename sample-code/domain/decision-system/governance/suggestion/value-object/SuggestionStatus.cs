using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.DecisionSystem.Governance.Suggestion;

public sealed class SuggestionStatus : ValueObject
{
    public static readonly SuggestionStatus Proposed = new("Proposed");
    public static readonly SuggestionStatus Reviewed = new("Reviewed");
    public static readonly SuggestionStatus Approved = new("Approved");
    public static readonly SuggestionStatus Rejected = new("Rejected");
    public static readonly SuggestionStatus Activated = new("Activated");
    public static readonly SuggestionStatus Withdrawn = new("Withdrawn");

    public string Value { get; }
    private SuggestionStatus(string value) => Value = value;

    public bool IsTerminal => this == Activated || this == Rejected || this == Withdrawn;

    public static bool IsValidTransition(SuggestionStatus from, SuggestionStatus to) =>
        (from, to) switch
        {
            _ when from == Proposed && to == Reviewed => true,
            _ when from == Proposed && to == Withdrawn => true,
            _ when from == Reviewed && to == Approved => true,
            _ when from == Reviewed && to == Rejected => true,
            _ when from == Approved && to == Activated => true,
            _ when from == Approved && to == Withdrawn => true,
            _ => false
        };

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
