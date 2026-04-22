using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.Sanction;

public readonly record struct EffectivePeriod
{
    public Timestamp EffectiveAt { get; }
    public Timestamp? ExpiresAt { get; }

    public EffectivePeriod(Timestamp effectiveAt, Timestamp? expiresAt)
    {
        if (expiresAt is { } end && end.Value <= effectiveAt.Value)
            throw new ArgumentException(
                "ExpiresAt must be strictly after EffectiveAt.", nameof(expiresAt));
        EffectiveAt = effectiveAt;
        ExpiresAt = expiresAt;
    }

    public static EffectivePeriod Open(Timestamp effectiveAt) => new(effectiveAt, null);
    public static EffectivePeriod Bounded(Timestamp effectiveAt, Timestamp expiresAt) => new(effectiveAt, expiresAt);
}
