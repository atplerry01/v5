using Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Access;
using Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Session;
using Whycespace.Domain.ContentSystem.Document.Governance.Retention;
using Whycespace.Domain.ContentSystem.Streaming.StreamCore.Stream;
using Whycespace.Domain.StructuralSystem.Cluster.Authority;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;
using AccessStreamRef = Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Access.StreamRef;

namespace Whycespace.Tests.Unit.CrossSystem;

/// <summary>
/// Cross-system policy failure scenarios.
/// Verifies that domain invariants correctly block operations when
/// temporal constraints, state preconditions, or policy rules are violated.
/// </summary>
public sealed class CrossSystemInvariantTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp Past = new(new DateTimeOffset(2026, 4, 21, 10, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp Now = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp Future = new(new DateTimeOffset(2026, 4, 22, 12, 0, 0, TimeSpan.Zero));

    // ── Temporal Window Invariants ────────────────────────────────────────────

    [Fact]
    public void AccessWindow_EndBeforeStart_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new AccessWindow(Future, Now));
    }

    [Fact]
    public void SessionWindow_ExpiryBeforeOpened_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new SessionWindow(Future, Now));
    }

    [Fact]
    public void RetentionWindow_ExpiresBeforeApplied_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new RetentionWindow(Future, Now));
    }

    [Fact]
    public void StreamAccessGrant_AlreadyExpiredWindow_Throws()
    {
        // Granting access at a time after the window has already ended is rejected
        var id = new StreamAccessId(IdGen.Generate("CS:access:past:id"));
        var streamRef = new AccessStreamRef(IdGen.Generate("CS:access:past:stream"));
        var window = new AccessWindow(Past, Now);
        var token = new TokenBinding("expired-window-token");

        // grantedAt = Future (after window end = Now) → HasExpired = true → throws
        Assert.ThrowsAny<Exception>(() =>
        {
            StreamAccessAggregate.Grant(id, streamRef, AccessMode.Read, window, token, Future);
        });
    }

    // ── State Machine Invariants ──────────────────────────────────────────────

    [Fact]
    public void IncidentAggregate_ClosedThenInvestigate_Throws()
    {
        var id = new IncidentId(IdGen.Generate("OPS:incident:closed:id"));
        var descriptor = new IncidentDescriptor("Disk failure on node-3", "P2");
        var aggregate = IncidentAggregate.Report(id, descriptor);
        aggregate.Investigate();
        aggregate.Resolve();
        aggregate.Close();

        Assert.ThrowsAny<Exception>(() => aggregate.Investigate());
    }

    [Fact]
    public void IncidentAggregate_ClosedThenResolve_Throws()
    {
        var id = new IncidentId(IdGen.Generate("OPS:incident:closed2:id"));
        var descriptor = new IncidentDescriptor("Memory leak in service B", "P3");
        var aggregate = IncidentAggregate.Report(id, descriptor);
        aggregate.Investigate();
        aggregate.Resolve();
        aggregate.Close();

        Assert.ThrowsAny<Exception>(() => aggregate.Resolve());
    }

    [Fact]
    public void StreamAggregate_EndedThenActivate_Throws()
    {
        var id = new StreamId(IdGen.Generate("CS:stream:ended:id"));
        var stream = StreamAggregate.Create(id, StreamMode.Live, StreamType.Video, Now);
        stream.Activate(Now);
        stream.End(Now);

        Assert.ThrowsAny<Exception>(() => stream.Activate(Now));
    }

    [Fact]
    public void StreamAggregate_ArchivedThenActivate_Throws()
    {
        var id = new StreamId(IdGen.Generate("CS:stream:archived:id"));
        var stream = StreamAggregate.Create(id, StreamMode.Live, StreamType.Video, Now);
        stream.Activate(Now);
        stream.End(Now);
        stream.Archive(Now);

        Assert.ThrowsAny<Exception>(() => stream.Activate(Now));
    }

    // ── Authority State Invariants ────────────────────────────────────────────

    [Fact]
    public void AuthorityAggregate_RevokedThenActivate_Throws()
    {
        var id = new AuthorityId(IdGen.Generate("SS:authority:revoked:id"));
        var clusterRef = new ClusterRef(IdGen.Generate("SS:authority:revoked:cluster"));
        var descriptor = new AuthorityDescriptor(clusterRef, "Revoked Authority");
        var aggregate = AuthorityAggregate.Establish(id, descriptor);
        aggregate.Activate();
        aggregate.Revoke();

        Assert.ThrowsAny<Exception>(() => aggregate.Activate());
    }

    [Fact]
    public void AuthorityAggregate_RetiredThenActivate_Throws()
    {
        var id = new AuthorityId(IdGen.Generate("SS:authority:retired:id"));
        var clusterRef = new ClusterRef(IdGen.Generate("SS:authority:retired:cluster"));
        var descriptor = new AuthorityDescriptor(clusterRef, "Retired Authority");
        var aggregate = AuthorityAggregate.Establish(id, descriptor);
        aggregate.Activate();
        aggregate.Retire();

        Assert.ThrowsAny<Exception>(() => aggregate.Activate());
    }

    [Fact]
    public void AuthorityAggregate_RetiredThenRevoke_Throws()
    {
        var id = new AuthorityId(IdGen.Generate("SS:authority:retired2:id"));
        var clusterRef = new ClusterRef(IdGen.Generate("SS:authority:retired2:cluster"));
        var descriptor = new AuthorityDescriptor(clusterRef, "Retired Authority 2");
        var aggregate = AuthorityAggregate.Establish(id, descriptor);
        aggregate.Activate();
        aggregate.Retire();

        Assert.ThrowsAny<Exception>(() => aggregate.Revoke());
    }

    // ── Value Object Invariants ───────────────────────────────────────────────

    [Fact]
    public void RetentionReason_Empty_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new RetentionReason(""));
        Assert.ThrowsAny<Exception>(() => new RetentionReason("   "));
    }

    [Fact]
    public void TokenBinding_Empty_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new TokenBinding(""));
        Assert.ThrowsAny<Exception>(() => new TokenBinding("  "));
    }

    [Fact]
    public void IncidentDescriptor_EmptyTitle_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            new IncidentDescriptor("", "P1"));
    }

    [Fact]
    public void IncidentDescriptor_EmptySeverity_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            new IncidentDescriptor("Valid Title", ""));
    }
}
