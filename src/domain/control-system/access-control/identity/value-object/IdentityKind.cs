namespace Whycespace.Domain.ControlSystem.AccessControl.Identity;

/// <summary>
/// Distinguishes system-level identities within the control-system administrative boundary.
/// Not user-facing identity (which belongs to trust-system); this represents service accounts,
/// admin operators, and system agents acting within the control plane.
/// </summary>
public enum IdentityKind
{
    ServiceAccount = 1,
    AdminOperator = 2,
    SystemAgent = 3
}
