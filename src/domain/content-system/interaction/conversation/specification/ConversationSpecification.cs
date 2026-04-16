using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Conversation;

public sealed record ConversationStartCandidate(ConversationId Id, ConversationTopic Topic, string InitiatorRef);

public sealed class ConversationSpecification : Specification<ConversationStartCandidate>
{
    public override bool IsSatisfiedBy(ConversationStartCandidate entity) =>
        entity is not null
        && entity.Id.Value != Guid.Empty
        && !string.IsNullOrWhiteSpace(entity.Topic.Value)
        && !string.IsNullOrWhiteSpace(entity.InitiatorRef);

    public void EnsureSatisfied(ConversationStartCandidate candidate)
    {
        if (candidate is null || string.IsNullOrWhiteSpace(candidate.InitiatorRef))
            throw ConversationErrors.InvalidParticipant();
        if (string.IsNullOrWhiteSpace(candidate.Topic.Value))
            throw ConversationErrors.InvalidTopic();
    }
}
