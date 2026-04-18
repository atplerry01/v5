using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Tests.Unit.EconomicSystem.Enforcement.Restriction;

/// <summary>
/// Phase 2 + 2.5 — EnforcementGuard semantics.
///
/// Phase 2 locked hard-reject for user-originated commands.
/// Phase 2.5 added a single narrow bypass: <c>isSystem=true</c> allows
/// workflow / compensation / recovery dispatches to complete even when
/// the subject has an active restriction. Every other path (null,
/// empty, High, Medium, unknown prefix) is a no-op.
/// </summary>
public sealed class EnforcementGuardTests
{
    [Fact]
    public void RequireNotRestricted_Null_IsNoOp()
    {
        EnforcementGuard.RequireNotRestricted(null, isSystem: false);
    }

    [Fact]
    public void RequireNotRestricted_Empty_IsNoOp()
    {
        EnforcementGuard.RequireNotRestricted(string.Empty, isSystem: false);
    }

    [Fact]
    public void RequireNotRestricted_HighSeverity_IsNoOp()
    {
        // High/Medium are escalation-level constraints, not restrictions.
        // The guard is restriction-only.
        EnforcementGuard.RequireNotRestricted("High", isSystem: false);
        EnforcementGuard.RequireNotRestricted("Medium", isSystem: false);
    }

    [Fact]
    public void RequireNotRestricted_RestrictedPrefix_ThrowsSubjectRestricted()
    {
        var ex = Assert.Throws<SubjectRestrictedException>(() =>
            EnforcementGuard.RequireNotRestricted("Restricted:WALLET", isSystem: false));

        Assert.Equal("WALLET", ex.Scope);
        Assert.Contains("scope=WALLET", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RequireNotRestricted_RestrictedPrefix_WithIsSystemTrue_BypassesGuard()
    {
        // Phase 2.5 — workflow/compensation bypass. Restriction constraint
        // is present but IsSystem=true must let the command through so the
        // workflow can converge. No throw, no state change.
        EnforcementGuard.RequireNotRestricted("Restricted:WALLET", isSystem: true);
    }

    [Fact]
    public void SubjectRestrictedException_IsDomainException_SoApiMapsTo400()
    {
        var ex = new SubjectRestrictedException("TRANSACTION", reason: "regulatory hold");

        Assert.IsAssignableFrom<DomainException>(ex);
        Assert.Equal("TRANSACTION", ex.Scope);
        Assert.Equal("regulatory hold", ex.Reason);
        Assert.Contains("regulatory hold", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RequireNotRestricted_UnknownPrefix_IsNoOp()
    {
        // Only the exact "Restricted:" prefix triggers hard reject; adjacent
        // spellings must not match.
        EnforcementGuard.RequireNotRestricted("restricted:wallet", isSystem: false); // lowercase
        EnforcementGuard.RequireNotRestricted("RESTRICTED:WALLET", isSystem: false); // uppercase
        EnforcementGuard.RequireNotRestricted("Restrict:wallet", isSystem: false);   // typo
    }

    [Fact]
    public void RequireNotRestricted_NoConstraint_WithIsSystemTrue_IsNoOp()
    {
        // Sanity: the bypass does not mask an error path — with no
        // constraint AND isSystem=true, still no throw.
        EnforcementGuard.RequireNotRestricted(null, isSystem: true);
    }
}
