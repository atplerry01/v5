namespace Whycespace.Domain.BusinessSystem.Logistic.Handoff;

public sealed record HandoffCreatedEvent(
    HandoffId HandoffId,
    ActorReference SourceActor,
    ActorReference TargetActor,
    TransferReference TransferReference);
