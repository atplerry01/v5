namespace Whycespace.Domain.BusinessSystem.Agreement.Amendment;

public sealed record AmendmentCreatedEvent(AmendmentId AmendmentId, AmendmentTargetId TargetId);
