using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Command.CommandEnvelope;

public sealed class CommandEnvelopeAggregate : AggregateRoot
{
    public CommandEnvelopeId Id { get; private set; }
    public EnvelopeMetadata Metadata { get; private set; }
    public CommandEnvelopeStatus Status { get; private set; }

    // ── Factory ──────────────────────────────────────────────────

    public static CommandEnvelopeAggregate Seal(
        CommandEnvelopeId id,
        EnvelopeMetadata metadata)
    {
        var aggregate = new CommandEnvelopeAggregate();
        if (aggregate.Version >= 0)
            throw CommandEnvelopeErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new CommandEnvelopeSealedEvent(id, metadata));
        return aggregate;
    }

    // ── Dispatch ─────────────────────────────────────────────────

    public void Dispatch()
    {
        var specification = new CanDispatchSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CommandEnvelopeErrors.InvalidStateTransition(Status, nameof(Dispatch));

        RaiseDomainEvent(new CommandEnvelopeDispatchedEvent(Id));
    }

    // ── Acknowledge ──────────────────────────────────────────────

    public void Acknowledge()
    {
        var specification = new CanAcknowledgeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CommandEnvelopeErrors.InvalidStateTransition(Status, nameof(Acknowledge));

        RaiseDomainEvent(new CommandEnvelopeAcknowledgedEvent(Id));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case CommandEnvelopeSealedEvent e:
                Id = e.EnvelopeId;
                Metadata = e.Metadata;
                Status = CommandEnvelopeStatus.Sealed;
                break;
            case CommandEnvelopeDispatchedEvent:
                Status = CommandEnvelopeStatus.Dispatched;
                break;
            case CommandEnvelopeAcknowledgedEvent:
                Status = CommandEnvelopeStatus.Acknowledged;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw CommandEnvelopeErrors.MissingId();

        if (Metadata == default)
            throw CommandEnvelopeErrors.MissingMetadata();

        if (!Enum.IsDefined(Status))
            throw CommandEnvelopeErrors.InvalidStateTransition(Status, "validate");
    }
}
