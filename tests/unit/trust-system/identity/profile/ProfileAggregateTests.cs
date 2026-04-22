using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.TrustSystem.Identity.Profile;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.TrustSystem.Identity.Profile;

public sealed class ProfileAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp FixedTs = new(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

    private static ProfileId NewId(string seed) =>
        new(IdGen.Generate($"ProfileAggregateTests:{seed}:profile"));

    private static ProfileDescriptor DefaultDescriptor() =>
        new(IdGen.Generate("ProfileAggregateTests:identity-ref"), "Alice Whycespace", "Personal");

    [Fact]
    public void Create_RaisesProfileCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var descriptor = DefaultDescriptor();

        var aggregate = ProfileAggregate.Create(id, descriptor, FixedTs);

        var evt = Assert.IsType<ProfileCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.ProfileId);
        Assert.Equal(descriptor.IdentityReference, evt.Descriptor.IdentityReference);
        Assert.Equal(descriptor.DisplayName, evt.Descriptor.DisplayName);
        Assert.Equal(descriptor.ProfileType, evt.Descriptor.ProfileType);
        Assert.Equal(FixedTs, evt.CreatedAt);
    }

    [Fact]
    public void Create_SetsStatusToCreated()
    {
        var aggregate = ProfileAggregate.Create(NewId("Status_Created"), DefaultDescriptor(), FixedTs);

        Assert.Equal(ProfileStatus.Created, aggregate.Status);
    }

    [Fact]
    public void Activate_FromCreated_SetsStatusToActive()
    {
        var aggregate = ProfileAggregate.Create(NewId("Activate_Created"), DefaultDescriptor(), FixedTs);
        aggregate.ClearDomainEvents();

        aggregate.Activate();

        Assert.IsType<ProfileActivatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(ProfileStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Deactivate_FromActive_SetsStatusToDeactivated()
    {
        var aggregate = ProfileAggregate.Create(NewId("Deactivate_Active"), DefaultDescriptor(), FixedTs);
        aggregate.Activate();
        aggregate.ClearDomainEvents();

        aggregate.Deactivate();

        Assert.IsType<ProfileDeactivatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(ProfileStatus.Deactivated, aggregate.Status);
    }

    [Fact]
    public void Deactivate_FromCreated_SetsStatusToDeactivated()
    {
        var aggregate = ProfileAggregate.Create(NewId("Deactivate_Created"), DefaultDescriptor(), FixedTs);
        aggregate.ClearDomainEvents();

        aggregate.Deactivate();

        Assert.Equal(ProfileStatus.Deactivated, aggregate.Status);
    }

    [Fact]
    public void Activate_FromDeactivated_Throws()
    {
        var aggregate = ProfileAggregate.Create(NewId("Activate_Deactivated"), DefaultDescriptor(), FixedTs);
        aggregate.Deactivate();

        Assert.ThrowsAny<Exception>(() => aggregate.Activate());
    }

    [Fact]
    public void Deactivate_WhenAlreadyDeactivated_Throws()
    {
        var aggregate = ProfileAggregate.Create(NewId("Deactivate_Again"), DefaultDescriptor(), FixedTs);
        aggregate.Deactivate();

        Assert.ThrowsAny<Exception>(() => aggregate.Deactivate());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();

        var history = new object[]
        {
            new ProfileCreatedEvent(id, descriptor, FixedTs),
            new ProfileActivatedEvent(id)
        };

        var aggregate = (ProfileAggregate)Activator.CreateInstance(typeof(ProfileAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.ProfileId);
        Assert.Equal(descriptor.IdentityReference, aggregate.Descriptor.IdentityReference);
        Assert.Equal(ProfileStatus.Active, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void LoadFromHistory_MatchesDirectConstruction()
    {
        var id = NewId("Replay");
        var descriptor = DefaultDescriptor();

        var direct = ProfileAggregate.Create(id, descriptor, FixedTs);
        direct.Activate();

        var replayed = (ProfileAggregate)Activator.CreateInstance(typeof(ProfileAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(direct.DomainEvents);

        Assert.Equal(direct.ProfileId, replayed.ProfileId);
        Assert.Equal(direct.Descriptor.IdentityReference, replayed.Descriptor.IdentityReference);
        Assert.Equal(direct.Status, replayed.Status);
    }
}
