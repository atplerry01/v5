using Whycespace.Domain.ControlSystem.AccessControl.Authorization;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.AccessControl.Authorization;

public sealed class AuthorizationAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly DateTimeOffset ValidFrom = new(2026, 4, 22, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ValidTo = new(2026, 12, 31, 0, 0, 0, TimeSpan.Zero);

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static AuthorizationId NewId(string seed) =>
        new(Hex64($"AuthorizationAggregateTests:{seed}:auth"));

    [Fact]
    public void Grant_RaisesAuthorizationGrantedEvent()
    {
        var id = NewId("Grant");

        var aggregate = AuthorizationAggregate.Grant(id, "subject-1", ["role-1"], ValidFrom, ValidTo);

        var evt = Assert.IsType<AuthorizationGrantedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("subject-1", evt.SubjectId);
        Assert.Contains("role-1", evt.RoleIds);
    }

    [Fact]
    public void Grant_SetsIsRevokedFalse()
    {
        var aggregate = AuthorizationAggregate.Grant(NewId("State"), "subject-1", ["role-1"], ValidFrom);

        Assert.False(aggregate.IsRevoked);
    }

    [Fact]
    public void Grant_WithEmptySubjectId_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            AuthorizationAggregate.Grant(NewId("EmptySubject"), "", ["role-1"], ValidFrom));
    }

    [Fact]
    public void Grant_WithNoRoles_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            AuthorizationAggregate.Grant(NewId("NoRoles"), "subject-1", [], ValidFrom));
    }

    [Fact]
    public void Grant_WithValidToBeforeValidFrom_Throws()
    {
        var invalidTo = ValidFrom.AddHours(-1);

        Assert.ThrowsAny<Exception>(() =>
            AuthorizationAggregate.Grant(NewId("InvalidDates"), "subject-1", ["role-1"], ValidFrom, invalidTo));
    }

    [Fact]
    public void Revoke_RaisesAuthorizationRevokedEvent()
    {
        var aggregate = AuthorizationAggregate.Grant(NewId("Revoke"), "subject-1", ["role-1"], ValidFrom);
        aggregate.ClearDomainEvents();

        aggregate.Revoke();

        Assert.IsType<AuthorizationRevokedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.True(aggregate.IsRevoked);
    }

    [Fact]
    public void Revoke_AlreadyRevoked_Throws()
    {
        var aggregate = AuthorizationAggregate.Grant(NewId("DoubleRevoke"), "subject-1", ["role-1"], ValidFrom);
        aggregate.Revoke();

        Assert.ThrowsAny<Exception>(() => aggregate.Revoke());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");
        var roleIds = new HashSet<string> { "role-1", "role-2" };

        var history = new object[]
        {
            new AuthorizationGrantedEvent(id, "subject-1", roleIds, ValidFrom, null)
        };
        var aggregate = (AuthorizationAggregate)Activator.CreateInstance(typeof(AuthorizationAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.False(aggregate.IsRevoked);
        Assert.Equal(2, aggregate.RoleIds.Count);
        Assert.Empty(aggregate.DomainEvents);
    }
}
