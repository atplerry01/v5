using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.DecisionSystem.Governance.Suggestion;

public sealed record SuggestionProposedEvent(Guid SuggestionId, string SuggestionType, string Description, string EngineSource, decimal Confidence) : DomainEvent;
public sealed record SuggestionReviewedEvent(Guid SuggestionId, string ReviewerId, string ReviewNotes) : DomainEvent;
public sealed record SuggestionApprovedEvent(Guid SuggestionId, string ApproverId) : DomainEvent;
public sealed record SuggestionRejectedEvent(Guid SuggestionId, string RejectorId, string Reason) : DomainEvent;
public sealed record SuggestionActivatedEvent(Guid SuggestionId) : DomainEvent;
public sealed record SuggestionWithdrawnEvent(Guid SuggestionId, string Reason) : DomainEvent;
