using Whycespace.Domain.ContentSystem.Document.CoreObject.File;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Document.CoreObject.File;

public sealed class DocumentFileAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static DocumentFileId NewId(string seed) =>
        new(IdGen.Generate($"DocumentFileAggregateTests:{seed}:file"));

    [Fact]
    public void Register_RaisesDocumentFileRegisteredEvent()
    {
        var id = NewId("Register_Valid");
        var docRef = new DocumentRef(IdGen.Generate("DocumentFileAggregateTests:doc-ref"));
        var storageRef = new DocumentFileStorageRef("s3://bucket/key/file.pdf");
        var checksum = new DocumentFileChecksum("a3b4c5d6e7f8a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a1b2c3d4e5f6a7b8");
        var mimeType = new DocumentFileMimeType("application/pdf");
        var size = new DocumentFileSize(204800);

        var aggregate = DocumentFileAggregate.Register(id, docRef, storageRef, checksum, mimeType, size, BaseTime);

        var evt = Assert.IsType<DocumentFileRegisteredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.DocumentFileId);
        Assert.Equal(docRef, evt.DocumentRef);
        Assert.Equal(mimeType, evt.MimeType);
    }

    [Fact]
    public void Register_SetsStateFromEvent()
    {
        var id = NewId("Register_State");
        var docRef = new DocumentRef(IdGen.Generate("DocumentFileAggregateTests:doc-state"));

        var aggregate = DocumentFileAggregate.Register(
            id, docRef,
            new DocumentFileStorageRef("s3://bucket/key2.pdf"),
            new DocumentFileChecksum("d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7"),
            new DocumentFileMimeType("application/pdf"),
            new DocumentFileSize(512000),
            BaseTime);

        Assert.Equal(id, aggregate.DocumentFileId);
        Assert.Equal(DocumentFileStatus.Registered, aggregate.Status);
    }

    [Fact]
    public void Register_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var docRef = new DocumentRef(IdGen.Generate("DocumentFileAggregateTests:stable-doc"));
        var f1 = DocumentFileAggregate.Register(id, docRef,
            new DocumentFileStorageRef("s3://bucket/stable.pdf"),
            new DocumentFileChecksum("e5f6a7b8c9d0e1f2a3b4c5d6e7f8a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8"),
            new DocumentFileMimeType("application/pdf"),
            new DocumentFileSize(1024), BaseTime);
        var f2 = DocumentFileAggregate.Register(id, docRef,
            new DocumentFileStorageRef("s3://bucket/stable.pdf"),
            new DocumentFileChecksum("e5f6a7b8c9d0e1f2a3b4c5d6e7f8a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8"),
            new DocumentFileMimeType("application/pdf"),
            new DocumentFileSize(1024), BaseTime);

        Assert.Equal(
            ((DocumentFileRegisteredEvent)f1.DomainEvents[0]).DocumentFileId.Value,
            ((DocumentFileRegisteredEvent)f2.DomainEvents[0]).DocumentFileId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesDocumentFileState()
    {
        var id = NewId("History");
        var docRef = new DocumentRef(IdGen.Generate("DocumentFileAggregateTests:history-doc"));

        var history = new object[]
        {
            new DocumentFileRegisteredEvent(id, docRef,
                new DocumentFileStorageRef("s3://bucket/history.pdf"),
                new DocumentFileChecksum("f6a7b8c9d0e1f2a3b4c5d6e7f8a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a1"),
                new DocumentFileMimeType("application/pdf"),
                new DocumentFileSize(2048), BaseTime)
        };

        var aggregate = (DocumentFileAggregate)Activator.CreateInstance(typeof(DocumentFileAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.DocumentFileId);
        Assert.Equal(DocumentFileStatus.Registered, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
