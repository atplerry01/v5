using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Messaging;

public sealed record MessageDispatchCandidate(
    MessageId MessageId,
    string ConversationRef,
    string SenderRef,
    MessageBody Body);

public sealed class MessagingSpecification : Specification<MessageDispatchCandidate>
{
    private static readonly IReadOnlyDictionary<MessageStatus, IReadOnlySet<MessageStatus>> Allowed =
        new Dictionary<MessageStatus, IReadOnlySet<MessageStatus>>
        {
            [MessageStatus.Draft] = new HashSet<MessageStatus> { MessageStatus.Sent, MessageStatus.Retracted },
            [MessageStatus.Sent] = new HashSet<MessageStatus> { MessageStatus.Delivered, MessageStatus.Retracted },
            [MessageStatus.Delivered] = new HashSet<MessageStatus> { MessageStatus.Read, MessageStatus.Retracted },
            [MessageStatus.Read] = new HashSet<MessageStatus> { MessageStatus.Retracted },
            [MessageStatus.Retracted] = new HashSet<MessageStatus>()
        };

    public override bool IsSatisfiedBy(MessageDispatchCandidate entity) =>
        entity is not null
        && entity.MessageId.Value != Guid.Empty
        && !string.IsNullOrWhiteSpace(entity.ConversationRef)
        && !string.IsNullOrWhiteSpace(entity.SenderRef)
        && !string.IsNullOrWhiteSpace(entity.Body.Value);

    public void EnsureSatisfied(MessageDispatchCandidate candidate)
    {
        if (candidate is null || string.IsNullOrWhiteSpace(candidate.SenderRef))
            throw MessagingErrors.InvalidSender();
        if (string.IsNullOrWhiteSpace(candidate.ConversationRef))
            throw MessagingErrors.InvalidConversation();
        if (string.IsNullOrWhiteSpace(candidate.Body.Value))
            throw MessagingErrors.InvalidBody();
    }

    public void EnsureTransition(MessageStatus from, MessageStatus to)
    {
        if (!Allowed.TryGetValue(from, out var set) || !set.Contains(to))
            throw MessagingErrors.InvalidTransition(from, to);
    }
}
