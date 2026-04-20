using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Session;

public readonly record struct SessionWindow
{
    public Timestamp OpenedAt { get; }
    public Timestamp ExpiresAt { get; }

    public SessionWindow(Timestamp openedAt, Timestamp expiresAt)
    {
        Guard.Against(
            expiresAt.Value <= openedAt.Value,
            "SessionWindow expiresAt must be after openedAt.");
        OpenedAt = openedAt;
        ExpiresAt = expiresAt;
    }

    public bool HasExpired(Timestamp now) => now.Value >= ExpiresAt.Value;
}
