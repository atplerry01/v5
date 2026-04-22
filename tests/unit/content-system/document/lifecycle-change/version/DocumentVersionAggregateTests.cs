using Whycespace.Domain.ContentSystem.Document.LifecycleChange.Version;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Document.LifecycleChange.Version;

public sealed class DocumentVersionAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static DocumentVersionId NewId(string seed) =>
        new(IdGen.Generate($"DocumentVersionAggregateTests:{seed}:version"));

    [Fact]
    public void Create_RaisesDocumentVersionCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var docRef = new DocumentRef(IdGen.Generate("DocumentVersionAggregateTests:doc-ref"));
        var artifactRef = new ArtifactRef(IdGen.Generate("DocumentVersionAggregateTests:artifact-ref"));
        var versionNumber = new VersionNumber(1, 0);

        var aggregate = DocumentVersionAggregate.Create(id, docRef, versionNumber, artifactRef, null, BaseTime);

        var evt = Assert.IsType<DocumentVersionCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.VersionId);
        Assert.Equal(docRef, evt.DocumentRef);
        Assert.Equal(versionNumber, evt.VersionNumber);
    }

    [Fact]
    public void Create_SetsStateFromEvent()
    {
        var id = NewId("Create_State");
        var docRef = new DocumentRef(IdGen.Generate("DocumentVersionAggregateTests:doc-state"));
        var artifactRef = new ArtifactRef(IdGen.Generate("DocumentVersionAggregateTests:artifact-state"));

        var aggregate = DocumentVersionAggregate.Create(id, docRef, new VersionNumber(1, 0), artifactRef, null, BaseTime);

        Assert.Equal(id, aggregate.VersionId);
        Assert.Equal(VersionStatus.Draft, aggregate.Status);
    }

    [Fact]
    public void Create_WithPreviousVersion_SetsLink()
    {
        var id = NewId("Create_WithPrev");
        var prevId = NewId("Prev");
        var docRef = new DocumentRef(IdGen.Generate("DocumentVersionAggregateTests:doc-prev"));
        var artifactRef = new ArtifactRef(IdGen.Generate("DocumentVersionAggregateTests:artifact-prev"));

        var aggregate = DocumentVersionAggregate.Create(id, docRef, new VersionNumber(2, 0), artifactRef, prevId, BaseTime);

        var evt = Assert.IsType<DocumentVersionCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(prevId, evt.PreviousVersionId);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var docRef = new DocumentRef(IdGen.Generate("DocumentVersionAggregateTests:stable-doc"));
        var artifactRef = new ArtifactRef(IdGen.Generate("DocumentVersionAggregateTests:stable-artifact"));
        var v1 = DocumentVersionAggregate.Create(id, docRef, new VersionNumber(1, 0), artifactRef, null, BaseTime);
        var v2 = DocumentVersionAggregate.Create(id, docRef, new VersionNumber(1, 0), artifactRef, null, BaseTime);

        Assert.Equal(
            ((DocumentVersionCreatedEvent)v1.DomainEvents[0]).VersionId.Value,
            ((DocumentVersionCreatedEvent)v2.DomainEvents[0]).VersionId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesDocumentVersionState()
    {
        var id = NewId("History");
        var docRef = new DocumentRef(IdGen.Generate("DocumentVersionAggregateTests:history-doc"));
        var artifactRef = new ArtifactRef(IdGen.Generate("DocumentVersionAggregateTests:history-artifact"));

        var history = new object[]
        {
            new DocumentVersionCreatedEvent(id, docRef, new VersionNumber(1, 0), artifactRef, null, BaseTime)
        };

        var aggregate = (DocumentVersionAggregate)Activator.CreateInstance(typeof(DocumentVersionAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.VersionId);
        Assert.Equal(VersionStatus.Draft, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
