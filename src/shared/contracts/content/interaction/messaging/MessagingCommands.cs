using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Interaction.Messaging;

public sealed record SendMessageCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid CorrelationId,
    Guid CausationId,
    Guid MessageId,
    string ConversationRef,
    string SenderRef,
    string Body,
    DateTimeOffset OccurredAt) : IHasAggregateId
{
    public Guid AggregateId => CommandId;
}

public sealed record MarkMessageDeliveredCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid CorrelationId,
    Guid CausationId,
    string RecipientRef,
    DateTimeOffset OccurredAt) : IHasAggregateId
{
    public Guid AggregateId => CommandId;
}

public sealed record MarkMessageReadCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid CorrelationId,
    Guid CausationId,
    string RecipientRef,
    DateTimeOffset OccurredAt) : IHasAggregateId
{
    public Guid AggregateId => CommandId;
}

public sealed record EditMessageCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid CorrelationId,
    Guid CausationId,
    string Body,
    DateTimeOffset OccurredAt) : IHasAggregateId
{
    public Guid AggregateId => CommandId;
}

public sealed record RetractMessageCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid CorrelationId,
    Guid CausationId,
    DateTimeOffset OccurredAt) : IHasAggregateId
{
    public Guid AggregateId => CommandId;
}
