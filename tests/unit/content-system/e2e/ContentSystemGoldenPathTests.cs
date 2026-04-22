using Whycespace.Domain.ContentSystem.Media.Intake.Ingest;
using Whycespace.Domain.ContentSystem.Media.CoreObject.Asset;
using Whycespace.Domain.ContentSystem.Document.CoreObject.Document;
using Whycespace.Domain.ContentSystem.Document.CoreObject.File;
using Whycespace.Domain.ContentSystem.Document.Governance.Retention;
using Whycespace.Domain.ContentSystem.Streaming.StreamCore.Stream;
using Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Broadcast;
using Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Access;
using BroadcastStreamRef = Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Broadcast.StreamRef;
using AccessStreamRef = Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Access.StreamRef;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.E2E;

/// <summary>
/// Golden path E2E tests for content-system:
///   1. Media ingest request → accept → processing start → complete
///   2. Document upload, register file, apply retention
///   3. Stream registration → schedule broadcast → grant access
///
/// These tests verify the full lifecycle chain across BCs without persistence.
/// Each step asserts events and aggregate state; final state is the authority.
/// </summary>
public sealed class ContentSystemGoldenPathTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp T0 = new(new DateTimeOffset(2026, 4, 22, 9, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp T1 = new(new DateTimeOffset(2026, 4, 22, 9, 5, 0, TimeSpan.Zero));
    private static readonly Timestamp T2 = new(new DateTimeOffset(2026, 4, 22, 9, 10, 0, TimeSpan.Zero));
    private static readonly Timestamp T3 = new(new DateTimeOffset(2026, 4, 22, 9, 15, 0, TimeSpan.Zero));
    private static readonly Timestamp T4 = new(new DateTimeOffset(2026, 4, 22, 11, 0, 0, TimeSpan.Zero));

    // ── Golden Path 1: Media Ingest Lifecycle ────────────────────────────────

    [Fact]
    public void MediaIngest_GoldenPath_RequestToComplete()
    {
        // Step 1 — request ingest
        var ingestId = new MediaIngestId(IdGen.Generate("GP:ingest:1:id"));
        var sourceRef = new MediaIngestSourceRef(IdGen.Generate("GP:ingest:1:source"));
        var inputRef = new MediaIngestInputRef(IdGen.Generate("GP:ingest:1:input"));

        var aggregate = MediaIngestAggregate.Request(ingestId, sourceRef, inputRef, T0);

        Assert.IsType<MediaIngestRequestedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(MediaIngestStatus.Requested, aggregate.Status);
        aggregate.ClearDomainEvents();

        // Step 2 — accept
        aggregate.Accept(T1);

        Assert.IsType<MediaIngestAcceptedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(MediaIngestStatus.Accepted, aggregate.Status);
        aggregate.ClearDomainEvents();

        // Step 3 — start processing
        aggregate.StartProcessing(T2);

        Assert.IsType<MediaIngestProcessingStartedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(MediaIngestStatus.Processing, aggregate.Status);
        aggregate.ClearDomainEvents();

        // Step 4 — complete with output
        var outputRef = new MediaIngestOutputRef(IdGen.Generate("GP:ingest:1:output"));
        aggregate.Complete(outputRef, T3);

        var completedEvt = Assert.IsType<MediaIngestCompletedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(MediaIngestStatus.Completed, aggregate.Status);
        Assert.Equal(outputRef, completedEvt.OutputRef);
    }

    [Fact]
    public void MediaIngest_GoldenPath_RequestThenFail()
    {
        var ingestId = new MediaIngestId(IdGen.Generate("GP:ingest:fail:id"));
        var aggregate = MediaIngestAggregate.Request(
            ingestId,
            new MediaIngestSourceRef(IdGen.Generate("GP:ingest:fail:source")),
            new MediaIngestInputRef(IdGen.Generate("GP:ingest:fail:input")),
            T0);
        aggregate.ClearDomainEvents();

        aggregate.Fail(new MediaIngestFailureReason("Source file not found."), T1);

        Assert.IsType<MediaIngestFailedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(MediaIngestStatus.Failed, aggregate.Status);

        // Terminal — no further transitions
        Assert.ThrowsAny<Exception>(() => aggregate.Accept(T2));
    }

    // ── Golden Path 2: Document Upload + Retention ───────────────────────────

    [Fact]
    public void Document_GoldenPath_CreateFileRegisterRetention()
    {
        var docId = new DocumentId(IdGen.Generate("GP:doc:1:id"));
        var structuralOwner = new StructuralOwnerRef(IdGen.Generate("GP:doc:1:owner"));
        var businessOwner = new BusinessOwnerRef(BusinessOwnerKind.Agreement, IdGen.Generate("GP:doc:1:ba"));

        // Step 1 — create document
        var document = DocumentAggregate.Create(
            docId, new DocumentTitle("Service Agreement Q2"),
            DocumentType.Report, DocumentClassification.Confidential,
            structuralOwner, businessOwner, T0);

        Assert.IsType<DocumentCreatedEvent>(Assert.Single(document.DomainEvents));
        Assert.Equal(DocumentStatus.Draft, document.Status);
        document.ClearDomainEvents();

        // Step 2 — register file
        var fileId = new DocumentFileId(IdGen.Generate("GP:doc:1:file"));
        var file = DocumentFileAggregate.Register(
            fileId,
            new DocumentRef(IdGen.Generate("GP:doc:1:docref")),
            new DocumentFileStorageRef("s3://contracts/q2-agreement.pdf"),
            new DocumentFileChecksum("b1c2d3e4f5a6b7c8d9e0f1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1c2"),
            new DocumentFileMimeType("application/pdf"),
            new DocumentFileSize(512_000),
            T1);

        var fileEvt = Assert.IsType<DocumentFileRegisteredEvent>(Assert.Single(file.DomainEvents));
        Assert.Equal(fileId, fileEvt.DocumentFileId);
        Assert.Equal(DocumentFileStatus.Registered, file.Status);

        // Step 3 — apply retention
        var retentionId = new RetentionId(IdGen.Generate("GP:doc:1:retention"));
        var retentionWindow = new RetentionWindow(T1, T4);
        var retention = RetentionAggregate.Apply(
            retentionId,
            new RetentionTargetRef(docId.Value, RetentionTargetKind.Document),
            retentionWindow,
            new RetentionReason("Regulatory requirement: 7-year financial record retention."),
            T2);

        var retentionEvt = Assert.IsType<RetentionAppliedEvent>(Assert.Single(retention.DomainEvents));
        Assert.Equal(retentionId, retentionEvt.RetentionId);
        Assert.Equal(RetentionStatus.Applied, retention.Status);
        Assert.Equal(retentionWindow.AppliedAt, retention.Window.AppliedAt);
        Assert.Equal(retentionWindow.ExpiresAt, retention.Window.ExpiresAt);
    }

    // ── Golden Path 3: Stream Registration + Broadcast + Access ─────────────

    [Fact]
    public void Stream_GoldenPath_CreateBroadcastGrantAccess()
    {
        // Step 1 — create stream
        var streamId = new StreamId(IdGen.Generate("GP:stream:1:id"));
        var stream = StreamAggregate.Create(streamId, StreamMode.Live, StreamType.Video, T0);

        Assert.IsType<StreamCreatedEvent>(Assert.Single(stream.DomainEvents));
        Assert.Equal(StreamStatus.Created, stream.Status);
        stream.ClearDomainEvents();

        // Step 2 — activate stream
        stream.Activate(T1);
        Assert.IsType<StreamActivatedEvent>(Assert.Single(stream.DomainEvents));
        Assert.Equal(StreamStatus.Active, stream.Status);

        // Step 3 — create broadcast
        var broadcastId = new BroadcastId(IdGen.Generate("GP:stream:1:broadcast"));
        var broadcast = BroadcastAggregate.Create(
            broadcastId, new BroadcastStreamRef(streamId.Value), T1);


        Assert.IsType<BroadcastCreatedEvent>(Assert.Single(broadcast.DomainEvents));
        Assert.Equal(BroadcastStatus.Created, broadcast.Status);
        broadcast.ClearDomainEvents();

        // Step 4 — schedule broadcast
        var broadcastWindow = new BroadcastWindow(T2, T4);
        broadcast.Schedule(broadcastWindow, T2);

        Assert.IsType<BroadcastScheduledEvent>(Assert.Single(broadcast.DomainEvents));
        Assert.Equal(BroadcastStatus.Scheduled, broadcast.Status);
        broadcast.ClearDomainEvents();

        // Step 5 — start broadcast
        broadcast.Start(T2);

        Assert.IsType<BroadcastStartedEvent>(Assert.Single(broadcast.DomainEvents));
        Assert.Equal(BroadcastStatus.Live, broadcast.Status);

        // Step 6 — grant access
        var accessId = new StreamAccessId(IdGen.Generate("GP:stream:1:access"));
        var accessWindow = new AccessWindow(T2, T4);
        var access = StreamAccessAggregate.Grant(
            accessId,
            new AccessStreamRef(streamId.Value),
            AccessMode.Read,
            accessWindow,
            new TokenBinding("viewer-jwt-token-abc123"),
            T2);

        var accessEvt = Assert.IsType<StreamAccessGrantedEvent>(Assert.Single(access.DomainEvents));
        Assert.Equal(accessId, accessEvt.AccessId);
        Assert.Equal(AccessMode.Read, accessEvt.Mode);
        Assert.Equal(AccessStatus.Granted, access.Status);
    }
}
