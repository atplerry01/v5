using Whycespace.Domain.ContentSystem.Media.Intake.Ingest;
using Whycespace.Domain.ContentSystem.Document.CoreObject.Document;
using Whycespace.Domain.ContentSystem.Streaming.StreamCore.Stream;
using Whycespace.Domain.StructuralSystem.Cluster.Cluster;
using Whycespace.Domain.StructuralSystem.Cluster.Authority;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.Determinism;

/// <summary>
/// Enforces WBSM v3 Determinism Rule ($9):
/// Same inputs (seed, command args) → identical event stream.
/// No Guid.NewGuid(), no DateTime.UtcNow in aggregate factories.
///
/// Each test creates the same aggregate twice with identical inputs and
/// asserts that every event's identity and field values are structurally equal.
/// </summary>
public sealed class DeterminismEnforcementTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp FixedTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    // ── MediaIngestAggregate ─────────────────────────────────────────────────

    [Fact]
    public void MediaIngest_SameInputs_ProducesIdenticalEventStream()
    {
        var id = new MediaIngestId(IdGen.Generate("DET:ingest:id"));
        var sourceRef = new MediaIngestSourceRef(IdGen.Generate("DET:ingest:source"));
        var inputRef = new MediaIngestInputRef(IdGen.Generate("DET:ingest:input"));

        var a1 = MediaIngestAggregate.Request(id, sourceRef, inputRef, FixedTime);
        var a2 = MediaIngestAggregate.Request(id, sourceRef, inputRef, FixedTime);

        var e1 = (MediaIngestRequestedEvent)a1.DomainEvents[0];
        var e2 = (MediaIngestRequestedEvent)a2.DomainEvents[0];

        Assert.Equal(e1.UploadId.Value, e2.UploadId.Value);
        Assert.Equal(e1.SourceRef.Value, e2.SourceRef.Value);
        Assert.Equal(e1.InputRef.Value, e2.InputRef.Value);
        Assert.Equal(e1.RequestedAt.Value, e2.RequestedAt.Value);
    }

    // ── DocumentAggregate ────────────────────────────────────────────────────

    [Fact]
    public void DocumentAggregate_SameInputs_ProducesIdenticalEventStream()
    {
        var id = new DocumentId(IdGen.Generate("DET:document:id"));
        var title = new DocumentTitle("Determinism Test");
        var structural = new StructuralOwnerRef(IdGen.Generate("DET:document:structural"));
        var business = new BusinessOwnerRef(BusinessOwnerKind.Agreement, IdGen.Generate("DET:document:business"));

        var d1 = DocumentAggregate.Create(id, title, DocumentType.Report, DocumentClassification.Internal, structural, business, FixedTime);
        var d2 = DocumentAggregate.Create(id, title, DocumentType.Report, DocumentClassification.Internal, structural, business, FixedTime);

        var e1 = (DocumentCreatedEvent)d1.DomainEvents[0];
        var e2 = (DocumentCreatedEvent)d2.DomainEvents[0];

        Assert.Equal(e1.DocumentId.Value, e2.DocumentId.Value);
        Assert.Equal(e1.Title.Value, e2.Title.Value);
        Assert.Equal(e1.Type, e2.Type);
        Assert.Equal(e1.Classification, e2.Classification);
        Assert.Equal(e1.StructuralOwner.Value, e2.StructuralOwner.Value);
        Assert.Equal(e1.BusinessOwner.Kind, e2.BusinessOwner.Kind);
        Assert.Equal(e1.BusinessOwner.Value, e2.BusinessOwner.Value);
    }

    // ── StreamAggregate ──────────────────────────────────────────────────────

    [Fact]
    public void StreamAggregate_SameInputs_ProducesIdenticalEventStream()
    {
        var id = new StreamId(IdGen.Generate("DET:stream:id"));

        var s1 = StreamAggregate.Create(id, StreamMode.Live, StreamType.Video, FixedTime);
        var s2 = StreamAggregate.Create(id, StreamMode.Live, StreamType.Video, FixedTime);

        var e1 = (StreamCreatedEvent)s1.DomainEvents[0];
        var e2 = (StreamCreatedEvent)s2.DomainEvents[0];

        Assert.Equal(e1.StreamId.Value, e2.StreamId.Value);
        Assert.Equal(e1.Mode, e2.Mode);
        Assert.Equal(e1.Type, e2.Type);
        Assert.Equal(e1.CreatedAt.Value, e2.CreatedAt.Value);
    }

    // ── ClusterAggregate ─────────────────────────────────────────────────────

    [Fact]
    public void ClusterAggregate_SameInputs_ProducesIdenticalEventStream()
    {
        var id = new ClusterId(IdGen.Generate("DET:cluster:id"));
        var descriptor = new ClusterDescriptor("Determinism Hub", "Primary");

        var c1 = ClusterAggregate.Define(id, descriptor);
        var c2 = ClusterAggregate.Define(id, descriptor);

        var e1 = (ClusterDefinedEvent)c1.DomainEvents[0];
        var e2 = (ClusterDefinedEvent)c2.DomainEvents[0];

        Assert.Equal(e1.ClusterId.Value, e2.ClusterId.Value);
        Assert.Equal(e1.Descriptor.ClusterName, e2.Descriptor.ClusterName);
        Assert.Equal(e1.Descriptor.ClusterType, e2.Descriptor.ClusterType);
    }

    // ── AuthorityAggregate ───────────────────────────────────────────────────

    [Fact]
    public void AuthorityAggregate_SameInputs_ProducesIdenticalEventStream()
    {
        var id = new AuthorityId(IdGen.Generate("DET:authority:id"));
        var clusterRef = new ClusterRef(IdGen.Generate("DET:authority:cluster"));
        var descriptor = new AuthorityDescriptor(clusterRef, "Deterministic Authority");

        var a1 = AuthorityAggregate.Establish(id, descriptor);
        var a2 = AuthorityAggregate.Establish(id, descriptor);

        var e1 = (AuthorityEstablishedEvent)a1.DomainEvents[0];
        var e2 = (AuthorityEstablishedEvent)a2.DomainEvents[0];

        Assert.Equal(e1.AuthorityId.Value, e2.AuthorityId.Value);
        Assert.Equal(e1.Descriptor.ClusterReference.Value, e2.Descriptor.ClusterReference.Value);
        Assert.Equal(e1.Descriptor.AuthorityName, e2.Descriptor.AuthorityName);
    }

    // ── Multi-step: determinism survives state transitions ───────────────────

    [Fact]
    public void ClusterAggregate_SameInputs_AfterActivate_ProducesIdenticalEventStreams()
    {
        var id = new ClusterId(IdGen.Generate("DET:cluster:activate:id"));
        var descriptor = new ClusterDescriptor("Activation Hub", "Secondary");

        var c1 = ClusterAggregate.Define(id, descriptor);
        c1.Activate();

        var c2 = ClusterAggregate.Define(id, descriptor);
        c2.Activate();

        // Event 0: ClusterDefinedEvent — same
        var def1 = (ClusterDefinedEvent)c1.DomainEvents[0];
        var def2 = (ClusterDefinedEvent)c2.DomainEvents[0];
        Assert.Equal(def1.ClusterId.Value, def2.ClusterId.Value);

        // Event 1: ClusterActivatedEvent — both carry same ClusterId
        var act1 = (ClusterActivatedEvent)c1.DomainEvents[1];
        var act2 = (ClusterActivatedEvent)c2.DomainEvents[1];
        Assert.Equal(act1.ClusterId.Value, act2.ClusterId.Value);

        // Final state is identical
        Assert.Equal(c1.Status, c2.Status);
        Assert.Equal(ClusterStatus.Active, c1.Status);
    }

    // ── TestIdGenerator itself is deterministic ──────────────────────────────

    [Fact]
    public void TestIdGenerator_SameSeed_ProducesIdenticalGuid()
    {
        var gen1 = new TestIdGenerator();
        var gen2 = new TestIdGenerator();

        var id1 = gen1.Generate("DET:seed:stability");
        var id2 = gen2.Generate("DET:seed:stability");

        Assert.Equal(id1, id2);
    }
}
