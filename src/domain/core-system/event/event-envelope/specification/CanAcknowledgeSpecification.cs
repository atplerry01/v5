namespace Whycespace.Domain.CoreSystem.Event.EventEnvelope;

public sealed class CanAcknowledgeSpecification
{
    public bool IsSatisfiedBy(EventEnvelopeStatus status)
    {
        return status == EventEnvelopeStatus.Published;
    }
}
