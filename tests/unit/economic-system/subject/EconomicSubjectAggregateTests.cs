using Whycespace.Domain.EconomicSystem.Subject.Subject;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Subject;

public sealed class EconomicSubjectAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static SubjectId NewId(string seed) =>
        new(IdGen.Generate($"EconomicSubjectTests:{seed}:subject"));

    [Fact]
    public void Register_Participant_RaisesRegisteredEvent()
    {
        var id = NewId("Participant");
        var structural = new StructuralRef(StructuralRefType.Participant, IdGen.Generate("EconomicSubjectTests:Participant:structural").ToString());
        var economic = new EconomicRef(EconomicRefType.CapitalAccount, IdGen.Generate("EconomicSubjectTests:Participant:economic").ToString());

        var aggregate = EconomicSubjectAggregate.Register(id, SubjectType.Participant, structural, economic);

        var evt = Assert.IsType<EconomicSubjectRegisteredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.SubjectId);
        Assert.Equal(SubjectType.Participant, evt.SubjectType);
    }

    [Fact]
    public void Register_SetsStateFromEvent()
    {
        var id = NewId("State");
        var structural = new StructuralRef(StructuralRefType.Cluster, IdGen.Generate("EconomicSubjectTests:State:structural").ToString());
        var economic = new EconomicRef(EconomicRefType.VaultAccount, IdGen.Generate("EconomicSubjectTests:State:economic").ToString());

        var aggregate = EconomicSubjectAggregate.Register(id, SubjectType.Cluster, structural, economic);

        Assert.Equal(id, aggregate.SubjectId);
        Assert.Equal(SubjectType.Cluster, aggregate.SubjectType);
        Assert.True(aggregate.IsRegistered);
    }

    [Fact]
    public void Register_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var structural = new StructuralRef(StructuralRefType.Provider, IdGen.Generate("EconomicSubjectTests:Stable:structural").ToString());
        var economic = new EconomicRef(EconomicRefType.CapitalAccount, IdGen.Generate("EconomicSubjectTests:Stable:economic").ToString());

        var a1 = EconomicSubjectAggregate.Register(id, SubjectType.Provider, structural, economic);
        var a2 = EconomicSubjectAggregate.Register(id, SubjectType.Provider, structural, economic);

        Assert.Equal(
            ((EconomicSubjectRegisteredEvent)a1.DomainEvents[0]).SubjectId.Value,
            ((EconomicSubjectRegisteredEvent)a2.DomainEvents[0]).SubjectId.Value);
    }

    [Fact]
    public void Register_InvalidRefTypeCombination_Throws()
    {
        var id = NewId("InvalidRef");
        var structural = new StructuralRef(StructuralRefType.Participant, "some-id");
        // Participant requires CapitalAccount, VaultAccount is invalid
        var economic = new EconomicRef(EconomicRefType.VaultAccount, "some-id");

        Assert.ThrowsAny<Exception>(() =>
            EconomicSubjectAggregate.Register(id, SubjectType.Participant, structural, economic));
    }

    [Fact]
    public void Register_NullStructuralRef_Throws()
    {
        var id = NewId("NullStructural");
        var economic = new EconomicRef(EconomicRefType.CapitalAccount, "some-id");

        Assert.ThrowsAny<Exception>(() =>
            EconomicSubjectAggregate.Register(id, SubjectType.Participant, null!, economic));
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");
        var structural = new StructuralRef(StructuralRefType.Spv, IdGen.Generate("EconomicSubjectTests:History:structural").ToString());
        var economic = new EconomicRef(EconomicRefType.VaultAccount, IdGen.Generate("EconomicSubjectTests:History:economic").ToString());

        var history = new object[]
        {
            new EconomicSubjectRegisteredEvent(id, SubjectType.SPV, structural, economic)
        };

        var aggregate = (EconomicSubjectAggregate)Activator.CreateInstance(typeof(EconomicSubjectAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.SubjectId);
        Assert.Equal(SubjectType.SPV, aggregate.SubjectType);
        Assert.True(aggregate.IsRegistered);
        Assert.Empty(aggregate.DomainEvents);
    }
}
