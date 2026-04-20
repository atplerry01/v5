namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Validity;

// Intentional: starts Valid on creation. A validity record is an in-force
// assertion by virtue of existing; Invalidate and Expire are the only exits.
public enum ValidityStatus
{
    Valid,
    Invalid,
    Expired
}
