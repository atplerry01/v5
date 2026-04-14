namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

/// <summary>
/// One participant's share within a distribution. Participant is identified
/// by a plain string id — the economic boundary never imports structural
/// types directly (bridge via EconomicSubject, see Part 8 of Phase 2C).
/// </summary>
public sealed record ParticipantShare
{
    public string ParticipantId { get; }
    public decimal Amount { get; }
    public decimal Percentage { get; }

    public ParticipantShare(string participantId, decimal amount, decimal percentage)
    {
        if (string.IsNullOrWhiteSpace(participantId))
            throw new ArgumentException("ParticipantId cannot be empty.", nameof(participantId));

        if (amount < 0m)
            throw new ArgumentException("ParticipantShare.Amount cannot be negative.", nameof(amount));

        if (percentage <= 0m || percentage > 100m)
            throw new ArgumentException(
                "ParticipantShare.Percentage must be in (0, 100].",
                nameof(percentage));

        ParticipantId = participantId;
        Amount = amount;
        Percentage = percentage;
    }
}
