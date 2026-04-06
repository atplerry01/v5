using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.DecisionSystem.Governance.Suggestion;

/// <summary>
/// Governance suggestion lifecycle: Proposed → Reviewed → Approved → Activated.
/// Intelligence engines produce suggestions; guardians review and approve.
/// This ensures controlled autonomy — intelligence advises, humans decide.
/// </summary>
public sealed class GovernanceSuggestionAggregate : AggregateRoot
{
    public string SuggestionType { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public SuggestionSource Source { get; private set; } = null!;
    public SuggestionConfidence Confidence { get; private set; } = null!;
    public SuggestionStatus Status { get; private set; } = SuggestionStatus.Proposed;
    public string? ReviewerId { get; private set; }
    public string? ReviewNotes { get; private set; }
    public string? ApproverId { get; private set; }

    public static GovernanceSuggestionAggregate Propose(
        Guid id, string suggestionType, string description,
        SuggestionSource source, SuggestionConfidence confidence)
    {
        var agg = new GovernanceSuggestionAggregate
        {
            Id = id,
            SuggestionType = suggestionType,
            Description = description,
            Source = source,
            Confidence = confidence,
            Status = SuggestionStatus.Proposed
        };
        agg.RaiseDomainEvent(new SuggestionProposedEvent(
            id, suggestionType, description, source.EngineId, confidence.Value));
        return agg;
    }

    public void Review(string reviewerId, string reviewNotes)
    {
        EnsureValidTransition(Status, SuggestionStatus.Reviewed, SuggestionStatus.IsValidTransition);
        ReviewerId = reviewerId;
        ReviewNotes = reviewNotes;
        Status = SuggestionStatus.Reviewed;
        RaiseDomainEvent(new SuggestionReviewedEvent(Id, reviewerId, reviewNotes));
    }

    public void Approve(string approverId)
    {
        EnsureValidTransition(Status, SuggestionStatus.Approved, SuggestionStatus.IsValidTransition);
        EnsureInvariant(
            Confidence.IsActionable,
            "ActionableConfidence",
            $"Cannot approve suggestion with confidence {Confidence.Value:F2} below actionable threshold.");
        ApproverId = approverId;
        Status = SuggestionStatus.Approved;
        RaiseDomainEvent(new SuggestionApprovedEvent(Id, approverId));
    }

    public void Reject(string rejectorId, string reason)
    {
        EnsureValidTransition(Status, SuggestionStatus.Rejected, SuggestionStatus.IsValidTransition);
        Status = SuggestionStatus.Rejected;
        RaiseDomainEvent(new SuggestionRejectedEvent(Id, rejectorId, reason));
    }

    public void Activate()
    {
        EnsureValidTransition(Status, SuggestionStatus.Activated, SuggestionStatus.IsValidTransition);
        Status = SuggestionStatus.Activated;
        RaiseDomainEvent(new SuggestionActivatedEvent(Id));
    }

    public void Withdraw(string reason)
    {
        EnsureNotTerminal(Status, s => s.IsTerminal, "Withdraw");
        Status = SuggestionStatus.Withdrawn;
        RaiseDomainEvent(new SuggestionWithdrawnEvent(Id, reason));
    }
}
