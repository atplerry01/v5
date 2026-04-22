using Whycespace.Domain.StructuralSystem.Cluster.Authority;
using Whycespace.Domain.StructuralSystem.Cluster.Cluster;
using Whycespace.Domain.StructuralSystem.Structure.Classification;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.StructuralSystem.ReplayLosslessness;

/// <summary>
/// INV-REPLAY-LOSSLESS-VALUEOBJECT-01
/// Verifies that LoadFromHistory produces structurally identical aggregate state
/// to direct factory construction — all VO fields survive the event round-trip.
/// </summary>
public sealed class StructuralSystemReplayLosslessnessTests
{
    private static readonly TestIdGenerator IdGen = new();

    // ── ClusterAggregate ──────────────────────────────────────────────────────

    [Fact]
    public void ClusterAggregate_Replay_PreservesDescriptorFields()
    {
        var id = new ClusterId(IdGen.Generate("LS:cluster:id"));
        var descriptor = new ClusterDescriptor("Northern Hub", "Primary");

        var direct = ClusterAggregate.Define(id, descriptor);

        var replayed = (ClusterAggregate)Activator.CreateInstance(typeof(ClusterAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(new object[] { new ClusterDefinedEvent(id, descriptor) });

        Assert.Equal(direct.ClusterId, replayed.ClusterId);
        Assert.Equal(direct.Descriptor.ClusterName, replayed.Descriptor.ClusterName);
        Assert.Equal(direct.Descriptor.ClusterType, replayed.Descriptor.ClusterType);
        Assert.Equal(direct.Status, replayed.Status);
    }

    [Fact]
    public void ClusterAggregate_Replay_AfterActivate_PreservesStatus()
    {
        var id = new ClusterId(IdGen.Generate("LS:cluster:activate"));
        var descriptor = new ClusterDescriptor("East Hub", "Secondary");

        var direct = ClusterAggregate.Define(id, descriptor);
        direct.Activate();

        var replayed = (ClusterAggregate)Activator.CreateInstance(typeof(ClusterAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(new object[]
        {
            new ClusterDefinedEvent(id, descriptor),
            new ClusterActivatedEvent(id)
        });

        Assert.Equal(direct.Status, replayed.Status);
        Assert.Equal(ClusterStatus.Active, replayed.Status);
    }

    // ── AuthorityAggregate ───────────────────────────────────────────────────

    [Fact]
    public void AuthorityAggregate_Replay_PreservesDescriptorFields()
    {
        var id = new AuthorityId(IdGen.Generate("LS:authority:id"));
        var clusterRef = new ClusterRef(IdGen.Generate("LS:authority:cluster"));
        var descriptor = new AuthorityDescriptor(clusterRef, "Finance Authority");

        var direct = AuthorityAggregate.Establish(id, descriptor);

        var replayed = (AuthorityAggregate)Activator.CreateInstance(typeof(AuthorityAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(new object[] { new AuthorityEstablishedEvent(id, descriptor) });

        Assert.Equal(direct.Id, replayed.Id);
        Assert.Equal(direct.Descriptor.ClusterReference, replayed.Descriptor.ClusterReference);
        Assert.Equal(direct.Descriptor.AuthorityName, replayed.Descriptor.AuthorityName);
        Assert.Equal(direct.Status, replayed.Status);
    }

    [Fact]
    public void AuthorityAggregate_Replay_AfterActivate_PreservesFullState()
    {
        var id = new AuthorityId(IdGen.Generate("LS:authority:activate"));
        var clusterRef = new ClusterRef(IdGen.Generate("LS:authority:cluster-act"));
        var descriptor = new AuthorityDescriptor(clusterRef, "HR Authority");

        var direct = AuthorityAggregate.Establish(id, descriptor);
        direct.Activate();

        var replayed = (AuthorityAggregate)Activator.CreateInstance(typeof(AuthorityAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(new object[]
        {
            new AuthorityEstablishedEvent(id, descriptor),
            new AuthorityActivatedEvent(id)
        });

        Assert.Equal(direct.Id, replayed.Id);
        Assert.Equal(direct.Descriptor, replayed.Descriptor);
        Assert.Equal(direct.Status, replayed.Status);
        Assert.Equal(AuthorityStatus.Active, replayed.Status);
    }

    // ── ClassificationAggregate ──────────────────────────────────────────────

    [Fact]
    public void ClassificationAggregate_Replay_PreservesDescriptorFields()
    {
        var id = new ClassificationId(IdGen.Generate("LS:classification:id"));
        var descriptor = new ClassificationDescriptor("Regulatory", "Tier-1");

        var direct = ClassificationAggregate.Define(id, descriptor);

        var replayed = (ClassificationAggregate)Activator.CreateInstance(typeof(ClassificationAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(new object[] { new ClassificationDefinedEvent(id, descriptor) });

        Assert.Equal(direct.Id, replayed.Id);
        Assert.Equal(direct.Descriptor.ClassificationName, replayed.Descriptor.ClassificationName);
        Assert.Equal(direct.Descriptor.ClassificationCategory, replayed.Descriptor.ClassificationCategory);
        Assert.Equal(direct.Status, replayed.Status);
    }

    // ── Cross-cutting: VO equality is structural, not reference ──────────────

    [Fact]
    public void ClusterDescriptor_StructuralEquality_SurvivesRoundTrip()
    {
        var id = new ClusterId(IdGen.Generate("LS:cluster:eq"));
        var descriptor = new ClusterDescriptor("West Cluster", "Tertiary");

        var evt = new ClusterDefinedEvent(id, descriptor);

        // Simulate extracting the descriptor from the event (as Apply would see it)
        var extractedDescriptor = evt.Descriptor;

        Assert.Equal(descriptor.ClusterName, extractedDescriptor.ClusterName);
        Assert.Equal(descriptor.ClusterType, extractedDescriptor.ClusterType);
        Assert.Equal(descriptor, extractedDescriptor);
    }

    [Fact]
    public void AuthorityDescriptor_ClusterRef_SurvivesRoundTrip()
    {
        var clusterRef = new ClusterRef(IdGen.Generate("LS:cluster-ref:eq"));
        var descriptor = new AuthorityDescriptor(clusterRef, "Legal Authority");

        var id = new AuthorityId(IdGen.Generate("LS:authority:eq"));
        var evt = new AuthorityEstablishedEvent(id, descriptor);

        var extractedRef = evt.Descriptor.ClusterReference;

        Assert.Equal(clusterRef.Value, extractedRef.Value);
        Assert.Equal(clusterRef, extractedRef);
    }
}
