namespace Whycespace.Shared.Contracts.Content.Interaction.Messaging;

public sealed record MessageReadModel
{
    public Guid Id { get; init; }
    public string ConversationRef { get; init; } = string.Empty;
    public string SenderRef { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string Status { get; init; } = "draft";
    public DateTimeOffset? SentAt { get; init; }
    public DateTimeOffset? LastTransitionedAt { get; init; }
    public string? LastRecipientRef { get; init; }
}
