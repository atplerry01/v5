using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Domain.StructuralSystem.Humancapital.Assignment;
using Whycespace.Domain.StructuralSystem.Humancapital.Participant;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.StructuralSystem.Humancapital.Assignment;

public sealed class AssignmentAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly DateTimeOffset BaseTime = new(2026, 4, 22, 10, 0, 0, TimeSpan.Zero);

    private static AssignmentId NewId(string seed) =>
        new(IdGen.Generate($"AssignmentAggregateTests:{seed}:assignment"));

    private static ParticipantId NewParticipantId(string seed) =>
        new(IdGen.Generate($"AssignmentAggregateTests:{seed}:participant").ToString());

    private static ClusterAuthorityRef NewAuthorityRef(string seed) =>
        new(IdGen.Generate($"AssignmentAggregateTests:{seed}:authority"));

    private sealed class StubParentLookup(StructuralParentState state) : IStructuralParentLookup
    {
        public StructuralParentState GetState(ClusterRef parent) => state;
        public StructuralParentState GetState(ClusterAuthorityRef parent) => state;
    }

    [Fact]
    public void Assign_WithActiveAuthority_RaisesAssignmentAssignedEvent()
    {
        var id = NewId("Assign_Valid");
        var participant = NewParticipantId("Assign_Valid");
        var authority = NewAuthorityRef("Assign_Valid");
        var lookup = new StubParentLookup(StructuralParentState.Active);

        var aggregate = AssignmentAggregate.Assign(id, participant, authority, BaseTime, lookup);

        var evt = Assert.IsType<AssignmentAssignedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.AssignmentId);
        Assert.Equal(participant, evt.Participant);
        Assert.Equal(authority, evt.Authority);
        Assert.Equal(BaseTime, evt.EffectiveAt);
    }

    [Fact]
    public void Assign_SetsStateFromEvent()
    {
        var id = NewId("Assign_State");
        var participant = NewParticipantId("Assign_State");
        var authority = NewAuthorityRef("Assign_State");
        var lookup = new StubParentLookup(StructuralParentState.Active);

        var aggregate = AssignmentAggregate.Assign(id, participant, authority, BaseTime, lookup);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(participant, aggregate.Participant);
        Assert.Equal(authority, aggregate.Authority);
        Assert.Equal(BaseTime, aggregate.EffectiveAt);
    }

    [Fact]
    public void Assign_WithInactiveAuthority_Throws()
    {
        var id = NewId("Assign_Inactive");
        var participant = NewParticipantId("Assign_Inactive");
        var authority = NewAuthorityRef("Assign_Inactive");
        var lookup = new StubParentLookup(StructuralParentState.Inactive);

        Assert.ThrowsAny<DomainException>(() =>
            AssignmentAggregate.Assign(id, participant, authority, BaseTime, lookup));
    }

    [Fact]
    public void Assign_WithSuspendedAuthority_Throws()
    {
        var id = NewId("Assign_Suspended");
        var participant = NewParticipantId("Assign_Suspended");
        var authority = NewAuthorityRef("Assign_Suspended");
        var lookup = new StubParentLookup(StructuralParentState.Suspended);

        Assert.ThrowsAny<DomainException>(() =>
            AssignmentAggregate.Assign(id, participant, authority, BaseTime, lookup));
    }

    [Fact]
    public void LoadFromHistory_RehydratesAssignmentState()
    {
        var id = NewId("History");
        var participant = NewParticipantId("History");
        var authority = NewAuthorityRef("History");

        var history = new object[]
        {
            new AssignmentAssignedEvent(id, participant, authority, BaseTime)
        };

        var aggregate = (AssignmentAggregate)Activator.CreateInstance(typeof(AssignmentAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(participant, aggregate.Participant);
        Assert.Equal(authority, aggregate.Authority);
        Assert.Empty(aggregate.DomainEvents);
    }
}
