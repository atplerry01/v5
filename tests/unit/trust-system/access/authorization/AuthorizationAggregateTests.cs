using Whycespace.Domain.TrustSystem.Access.Authorization;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.TrustSystem.Access.Authorization;

public sealed class AuthorizationAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static AuthorizationId NewId(string seed) =>
        new(IdGen.Generate($"AuthorizationAggregateTests:{seed}:authorization"));

    private static AuthorizationScope DefaultScope(string seed) =>
        new(IdGen.Generate($"AuthorizationAggregateTests:{seed}:principal"), "document:read");

    [Fact]
    public void Grant_RaisesAuthorizationGrantedEvent()
    {
        var id = NewId("Grant_Valid");
        var scope = DefaultScope("Grant_Valid");

        var aggregate = AuthorizationAggregate.Grant(id, scope);

        var evt = Assert.IsType<AuthorizationGrantedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.AuthorizationId);
        Assert.Equal(scope.ResourceReference, evt.Scope.ResourceReference);
    }

    [Fact]
    public void Grant_SetsStatusToGranted()
    {
        var aggregate = AuthorizationAggregate.Grant(NewId("State_Grant"), DefaultScope("State_Grant"));

        Assert.Equal(AuthorizationStatus.Granted, aggregate.Status);
    }

    [Fact]
    public void Deny_SetsStatusToDenied()
    {
        var aggregate = AuthorizationAggregate.Deny(NewId("Deny"), DefaultScope("Deny"));

        Assert.Equal(AuthorizationStatus.Denied, aggregate.Status);
    }

    [Fact]
    public void Grant_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var scope = DefaultScope("Stable");

        var a1 = AuthorizationAggregate.Grant(id, scope);
        var a2 = AuthorizationAggregate.Grant(id, scope);

        Assert.Equal(
            ((AuthorizationGrantedEvent)a1.DomainEvents[0]).AuthorizationId.Value,
            ((AuthorizationGrantedEvent)a2.DomainEvents[0]).AuthorizationId.Value);
    }

    [Fact]
    public void Revoke_FromGranted_SetsStatusToRevoked()
    {
        var aggregate = AuthorizationAggregate.Grant(NewId("Revoke"), DefaultScope("Revoke"));
        aggregate.ClearDomainEvents();

        aggregate.Revoke();

        Assert.IsType<AuthorizationRevokedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(AuthorizationStatus.Revoked, aggregate.Status);
    }

    [Fact]
    public void Revoke_AfterRevoke_Throws()
    {
        var aggregate = AuthorizationAggregate.Grant(NewId("Revoke_Again"), DefaultScope("Revoke_Again"));
        aggregate.Revoke();

        Assert.ThrowsAny<Exception>(() => aggregate.Revoke());
    }

    [Fact]
    public void LoadFromHistory_Granted_RehydratesState()
    {
        var id = NewId("History_Grant");
        var scope = DefaultScope("History_Grant");

        var history = new object[]
        {
            new AuthorizationGrantedEvent(id, scope)
        };

        var aggregate = (AuthorizationAggregate)Activator.CreateInstance(typeof(AuthorizationAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(AuthorizationStatus.Granted, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void LoadFromHistory_Denied_RehydratesState()
    {
        var id = NewId("History_Deny");
        var scope = DefaultScope("History_Deny");

        var history = new object[]
        {
            new AuthorizationDeniedEvent(id, scope)
        };

        var aggregate = (AuthorizationAggregate)Activator.CreateInstance(typeof(AuthorizationAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(AuthorizationStatus.Denied, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
