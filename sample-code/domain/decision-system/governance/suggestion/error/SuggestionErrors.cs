using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.DecisionSystem.Governance.Suggestion;

public sealed class SuggestionTerminalException : DomainException
{
    public SuggestionTerminalException(string status)
        : base("SUGGESTION_TERMINAL", $"Suggestion is in terminal state: {status}.") { }
}

public sealed class InsufficientConfidenceException : DomainException
{
    public InsufficientConfidenceException(decimal confidence)
        : base("INSUFFICIENT_CONFIDENCE", $"Suggestion confidence {confidence:F2} is below actionable threshold.") { }
}
