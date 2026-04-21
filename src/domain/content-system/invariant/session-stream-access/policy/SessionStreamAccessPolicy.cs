namespace Whycespace.Domain.ContentSystem.Invariant.SessionStreamAccess;

/// <summary>
/// Cross-system invariant: a playback-consumption/session may be activated
/// only if delivery-governance/access has a non-revoked, non-expired grant
/// for the (subject, stream) pair at the activation instant.
///
/// Spans two BCs in the streaming context. Lives here, not on the Session
/// aggregate, because the access grant's authoritative state is owned by the
/// StreamAccess aggregate and must be observed at the evaluation instant.
///
/// Pure decision function per 02-engine-skeleton.md § Cross-System
/// Invariants. Intended to be invoked in ActivateSessionHandler (T2E) before
/// aggregate mutation.
/// </summary>
public sealed class SessionStreamAccessPolicy
{
    public SessionStreamAccessDecision Evaluate(SessionStreamAccessInput input)
    {
        if (input.StreamId == Guid.Empty)
            return SessionStreamAccessDecision.Deny(SessionStreamAccessReason.MissingStream);

        if (input.SubjectId == Guid.Empty)
            return SessionStreamAccessDecision.Deny(SessionStreamAccessReason.MissingSubject);

        if (!input.AccessGrantExists)
            return SessionStreamAccessDecision.Deny(SessionStreamAccessReason.AccessGrantMissing);

        if (input.AccessGrantRevoked)
            return SessionStreamAccessDecision.Deny(SessionStreamAccessReason.AccessGrantRevoked);

        if (input.AccessGrantExpired)
            return SessionStreamAccessDecision.Deny(SessionStreamAccessReason.AccessGrantExpired);

        if (input.AccessGrantRestricted)
            return SessionStreamAccessDecision.Deny(SessionStreamAccessReason.AccessGrantRestricted);

        return SessionStreamAccessDecision.Allow();
    }
}
