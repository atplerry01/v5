namespace Whycespace.Domain.CoreSystem.Event.EventEnvelope;

public sealed class CanPublishSpecification
{
    public bool IsSatisfiedBy(EventEnvelopeStatus status)
    {
        return status == EventEnvelopeStatus.Sealed;
    }
}
