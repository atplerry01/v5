using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.TrustSystem.Identity.Device;

public sealed class DeviceTrust : Entity
{
    public Guid DeviceId { get; private set; }
    public decimal TrustScore { get; private set; }
    public DateTimeOffset EvaluatedAt { get; private set; }

    private DeviceTrust() { }

    public static DeviceTrust Evaluate(Guid deviceId, decimal trustScore, DateTimeOffset timestamp)
    {
        return new DeviceTrust
        {
            Id = DeterministicIdHelper.FromSeed($"DeviceTrust:{deviceId}:{trustScore}"),
            DeviceId = deviceId,
            TrustScore = Math.Clamp(trustScore, 0m, 100m),
            EvaluatedAt = timestamp
        };
    }
}
