namespace Whycespace.Domain.ControlSystem.Enforcement.Lock;

/// <summary>
/// Phase 7 T7.6 — lock lifecycle.
///
/// Transitions (all other pairs are rejected by the aggregate):
///   Lock       →  Locked
///   Suspend    :  Locked    → Suspended
///   Resume     :  Suspended → Locked
///   Unlock     :  Locked | Suspended → Unlocked  (terminal — explicit release)
///   Expire     :  Locked             → Expired   (terminal — natural expiry)
///
/// Suspended is a reversible pause; Unlocked and Expired are terminal
/// and irreversible. Expiry is never raised from Suspended — a suspended
/// lock's timer is conceptually paused and must be Resumed before it
/// can expire.
/// </summary>
public enum LockStatus
{
    Locked,
    Suspended,
    Unlocked,
    Expired
}
