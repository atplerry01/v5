using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Retention;

public readonly record struct RetentionWindow
{
    public Timestamp AppliedAt { get; }
    public Timestamp ExpiresAt { get; }

    public RetentionWindow(Timestamp appliedAt, Timestamp expiresAt)
    {
        Guard.Against(
            expiresAt.Value <= appliedAt.Value,
            "RetentionWindow expiresAt must be after appliedAt.");
        AppliedAt = appliedAt;
        ExpiresAt = expiresAt;
    }

    public bool HasExpired(Timestamp now) => now.Value >= ExpiresAt.Value;
}
