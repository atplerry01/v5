namespace Whycespace.Domain.CoreSystem.Command.CommandEnvelope;

public sealed class CanAcknowledgeSpecification
{
    public bool IsSatisfiedBy(CommandEnvelopeStatus status)
    {
        return status == CommandEnvelopeStatus.Dispatched;
    }
}
