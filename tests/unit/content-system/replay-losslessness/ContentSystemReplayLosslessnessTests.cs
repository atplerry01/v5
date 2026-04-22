using Whycespace.Domain.ContentSystem.Document.CoreObject.Document;
using Whycespace.Domain.ContentSystem.Document.Governance.Retention;
using Whycespace.Domain.ContentSystem.Media.Intake.Ingest;
using Whycespace.Domain.ContentSystem.Streaming.StreamCore.Stream;
using Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Access;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;
using SessionNs = Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Session;

namespace Whycespace.Tests.Unit.ContentSystem.ReplayLosslessness;

/// <summary>
/// INV-REPLAY-LOSSLESS-VALUEOBJECT-01 — content-system
/// Verifies that LoadFromHistory produces structurally identical aggregate state
/// to direct factory construction — all VO fields survive the event round-trip.
/// </summary>
public sealed class ContentSystemReplayLosslessnessTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp FutureTime = new(new DateTimeOffset(2026, 4, 22, 12, 0, 0, TimeSpan.Zero));

    // ── MediaIngestAggregate ─────────────────────────────────────────────────

    [Fact]
    public void MediaIngestAggregate_Replay_PreservesAllVoFields()
    {
        var id = new MediaIngestId(IdGen.Generate("LS:ingest:id"));
        var sourceRef = new MediaIngestSourceRef(IdGen.Generate("LS:ingest:source"));
        var inputRef = new MediaIngestInputRef(IdGen.Generate("LS:ingest:input"));

        var direct = MediaIngestAggregate.Request(id, sourceRef, inputRef, BaseTime);

        var replayed = (MediaIngestAggregate)Activator.CreateInstance(typeof(MediaIngestAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(new object[]
        {
            new MediaIngestRequestedEvent(id, sourceRef, inputRef, BaseTime)
        });

        Assert.Equal(direct.UploadId, replayed.UploadId);
        Assert.Equal(direct.SourceRef, replayed.SourceRef);
        Assert.Equal(direct.InputRef, replayed.InputRef);
        Assert.Equal(direct.Status, replayed.Status);
        Assert.Equal(direct.RequestedAt, replayed.RequestedAt);
    }

    // ── DocumentAggregate ────────────────────────────────────────────────────

    [Fact]
    public void DocumentAggregate_Replay_PreservesAllVoFields()
    {
        var id = new DocumentId(IdGen.Generate("LS:document:id"));
        var title = new DocumentTitle("Losslessness Test Document");
        var structuralOwner = new StructuralOwnerRef(IdGen.Generate("LS:document:structural"));
        var businessOwner = new BusinessOwnerRef(BusinessOwnerKind.Agreement, IdGen.Generate("LS:document:business"));

        var direct = DocumentAggregate.Create(
            id, title, DocumentType.Report, DocumentClassification.Internal,
            structuralOwner, businessOwner, BaseTime);

        var replayed = (DocumentAggregate)Activator.CreateInstance(typeof(DocumentAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(new object[]
        {
            new DocumentCreatedEvent(id, title, DocumentType.Report, DocumentClassification.Internal,
                structuralOwner, businessOwner, BaseTime)
        });

        Assert.Equal(direct.DocumentId, replayed.DocumentId);
        Assert.Equal(direct.Title, replayed.Title);
        Assert.Equal(direct.Type, replayed.Type);
        Assert.Equal(direct.Classification, replayed.Classification);
        Assert.Equal(direct.StructuralOwner, replayed.StructuralOwner);
        Assert.Equal(direct.BusinessOwner.Kind, replayed.BusinessOwner.Kind);
        Assert.Equal(direct.BusinessOwner.Value, replayed.BusinessOwner.Value);
        Assert.Equal(direct.Status, replayed.Status);
    }

    // ── RetentionAggregate ───────────────────────────────────────────────────

    [Fact]
    public void RetentionAggregate_Replay_PreservesWindowAndReason()
    {
        var id = new RetentionId(IdGen.Generate("LS:retention:id"));
        var targetRef = new RetentionTargetRef(IdGen.Generate("LS:retention:target"), RetentionTargetKind.Document);
        var window = new RetentionWindow(BaseTime, FutureTime);
        var reason = new RetentionReason("Legal hold per regulation 42-B.");

        var direct = RetentionAggregate.Apply(id, targetRef, window, reason, BaseTime);

        var replayed = (RetentionAggregate)Activator.CreateInstance(typeof(RetentionAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(new object[]
        {
            new RetentionAppliedEvent(id, targetRef, window, reason, BaseTime)
        });

        Assert.Equal(direct.RetentionId, replayed.RetentionId);
        Assert.Equal(direct.TargetRef.Value, replayed.TargetRef.Value);
        Assert.Equal(direct.TargetRef.Kind, replayed.TargetRef.Kind);
        Assert.Equal(direct.Window.AppliedAt, replayed.Window.AppliedAt);
        Assert.Equal(direct.Window.ExpiresAt, replayed.Window.ExpiresAt);
        Assert.Equal(direct.Reason.Value, replayed.Reason.Value);
        Assert.Equal(direct.Status, replayed.Status);
    }

    // ── StreamAggregate ──────────────────────────────────────────────────────

    [Fact]
    public void StreamAggregate_Replay_PreservesAllVoFields()
    {
        var id = new StreamId(IdGen.Generate("LS:stream:id"));

        var direct = StreamAggregate.Create(id, StreamMode.Live, StreamType.Video, BaseTime);

        var replayed = (StreamAggregate)Activator.CreateInstance(typeof(StreamAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(new object[]
        {
            new StreamCreatedEvent(id, StreamMode.Live, StreamType.Video, BaseTime)
        });

        Assert.Equal(direct.StreamId, replayed.StreamId);
        Assert.Equal(direct.Mode, replayed.Mode);
        Assert.Equal(direct.Type, replayed.Type);
        Assert.Equal(direct.Status, replayed.Status);
    }

    // ── SessionAggregate ─────────────────────────────────────────────────────

    [Fact]
    public void SessionAggregate_Replay_PreservesWindowFields()
    {
        var id = new SessionNs.SessionId(IdGen.Generate("LS:session:id"));
        var streamRef = new SessionNs.StreamRef(IdGen.Generate("LS:session:stream"));
        var window = new SessionNs.SessionWindow(BaseTime, FutureTime);

        var direct = SessionNs.SessionAggregate.Open(id, streamRef, window, BaseTime);

        var replayed = (SessionNs.SessionAggregate)Activator.CreateInstance(typeof(SessionNs.SessionAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(new object[]
        {
            new SessionNs.SessionOpenedEvent(id, streamRef, window, BaseTime)
        });

        Assert.Equal(direct.SessionId, replayed.SessionId);
        Assert.Equal(direct.StreamRef, replayed.StreamRef);
        Assert.Equal(direct.Window.OpenedAt, replayed.Window.OpenedAt);
        Assert.Equal(direct.Window.ExpiresAt, replayed.Window.ExpiresAt);
        Assert.Equal(direct.Status, replayed.Status);
    }

    // ── StreamAccessAggregate ────────────────────────────────────────────────

    [Fact]
    public void StreamAccessAggregate_Replay_PreservesTokenAndWindow()
    {
        var id = new StreamAccessId(IdGen.Generate("LS:access:id"));
        var streamRef = new StreamRef(IdGen.Generate("LS:access:stream"));
        var window = new AccessWindow(BaseTime, FutureTime);
        var token = new TokenBinding("losslessness-token-abc123");

        var direct = StreamAccessAggregate.Grant(id, streamRef, AccessMode.Read, window, token, BaseTime);

        var replayed = (StreamAccessAggregate)Activator.CreateInstance(typeof(StreamAccessAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(new object[]
        {
            new StreamAccessGrantedEvent(id, streamRef, AccessMode.Read, window, token, BaseTime)
        });

        Assert.Equal(direct.AccessId, replayed.AccessId);
        Assert.Equal(direct.StreamRef, replayed.StreamRef);
        Assert.Equal(direct.Mode, replayed.Mode);
        Assert.Equal(direct.Token.Value, replayed.Token.Value);
        Assert.Equal(direct.Window.Start, replayed.Window.Start);
        Assert.Equal(direct.Window.End, replayed.Window.End);
        Assert.Equal(direct.Status, replayed.Status);
    }
}
