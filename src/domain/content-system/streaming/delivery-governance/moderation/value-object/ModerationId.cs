using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Moderation;

public readonly record struct ModerationId
{
    public Guid Value { get; }

    public ModerationId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ModerationId cannot be empty.");
        Value = value;
    }
}
