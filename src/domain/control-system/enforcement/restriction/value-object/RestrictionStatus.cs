namespace Whycespace.Domain.ControlSystem.Enforcement.Restriction;

/// <summary>
/// Phase 7 T7.6 — restriction lifecycle.
///
/// Transitions (all other pairs are rejected by the aggregate):
///   Apply      →  Applied
///   Suspend    :  Applied   → Suspended
///   Resume     :  Suspended → Applied
///   Update     :  Applied            (Suspended / Removed rejected)
///   Remove     :  Applied | Suspended → Removed  (terminal)
///
/// Suspended is a reversible pause; Removed is terminal and irreversible.
/// </summary>
public enum RestrictionStatus
{
    Applied,
    Suspended,
    Removed
}
