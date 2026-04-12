namespace Whycespace.Domain.CoreSystem.Command.CommandEnvelope;

public sealed class CanDispatchSpecification
{
    public bool IsSatisfiedBy(CommandEnvelopeStatus status)
    {
        return status == CommandEnvelopeStatus.Sealed;
    }
}
