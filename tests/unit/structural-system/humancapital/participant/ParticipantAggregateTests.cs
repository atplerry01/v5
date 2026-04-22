using Whycespace.Domain.StructuralSystem.Humancapital.Participant;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.StructuralSystem.Humancapital.Participant;

public sealed class ParticipantAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly DateTimeOffset BaseTime =
        new(2026, 4, 22, 10, 0, 0, TimeSpan.Zero);

    private static ParticipantId NewId(string seed) =>
        new(IdGen.Generate($"ParticipantAggregateTests:{seed}:participant").ToString());

    [Fact]
    public void Register_RaisesParticipantRegisteredEvent()
    {
        var id = NewId("Register_Valid");

        var aggregate = ParticipantAggregate.Register(id);

        var evt = Assert.IsType<ParticipantRegisteredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id.Value, evt.ParticipantId);
    }

    [Fact]
    public void Register_SetsIdOnAggregate()
    {
        var id = NewId("Register_State");

        var aggregate = ParticipantAggregate.Register(id);

        Assert.Equal(id, aggregate.Id);
        Assert.Null(aggregate.HomeCluster);
        Assert.Null(aggregate.PlacedAt);
    }

    [Fact]
    public void Register_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var p1 = ParticipantAggregate.Register(id);
        var p2 = ParticipantAggregate.Register(id);

        Assert.Equal(
            ((ParticipantRegisteredEvent)p1.DomainEvents[0]).ParticipantId,
            ((ParticipantRegisteredEvent)p2.DomainEvents[0]).ParticipantId);
    }

    [Fact]
    public void LoadFromHistory_Register_RehydratesId()
    {
        var id = NewId("History_Register");

        var history = new object[]
        {
            new ParticipantRegisteredEvent(id.Value)
        };

        var aggregate = (ParticipantAggregate)Activator.CreateInstance(typeof(ParticipantAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Null(aggregate.HomeCluster);
        Assert.Empty(aggregate.DomainEvents);
    }
}
