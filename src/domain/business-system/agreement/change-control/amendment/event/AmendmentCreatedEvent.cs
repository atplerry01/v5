namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Amendment;

public sealed record AmendmentCreatedEvent(AmendmentId AmendmentId, AmendmentTargetId TargetId);
