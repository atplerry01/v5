using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.TrustSystem.Access.Request;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.TrustSystem.Access.Request;

public sealed class RequestAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp FixedTs = new(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

    private static RequestId NewId(string seed) =>
        new(IdGen.Generate($"RequestAggregateTests:{seed}:request"));

    private static RequestDescriptor DefaultDescriptor() =>
        new(IdGen.Generate("RequestAggregateTests:principal-ref"), "AccessRequest", "admin-panel");

    [Fact]
    public void Submit_RaisesRequestSubmittedEvent()
    {
        var id = NewId("Submit_Valid");
        var descriptor = DefaultDescriptor();
        var aggregate = RequestAggregate.Submit(id, descriptor, FixedTs);
        var evt = Assert.IsType<RequestSubmittedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.RequestId);
        Assert.Equal(descriptor.RequestType, evt.Descriptor.RequestType);
    }

    [Fact]
    public void Submit_SetsStatusToSubmitted()
    {
        var aggregate = RequestAggregate.Submit(NewId("Status"), DefaultDescriptor(), FixedTs);
        Assert.Equal(RequestStatus.Submitted, aggregate.Status);
    }

    [Fact]
    public void Approve_FromSubmitted_SetsStatusToApproved()
    {
        var aggregate = RequestAggregate.Submit(NewId("Approve"), DefaultDescriptor(), FixedTs);
        aggregate.ClearDomainEvents();
        aggregate.Approve();
        Assert.IsType<RequestApprovedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(RequestStatus.Approved, aggregate.Status);
    }

    [Fact]
    public void Deny_FromSubmitted_SetsStatusToDenied()
    {
        var aggregate = RequestAggregate.Submit(NewId("Deny"), DefaultDescriptor(), FixedTs);
        aggregate.ClearDomainEvents();
        aggregate.Deny("Insufficient privileges.");
        var evt = Assert.IsType<RequestDeniedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal("Insufficient privileges.", evt.Reason);
        Assert.Equal(RequestStatus.Denied, aggregate.Status);
    }

    [Fact]
    public void Withdraw_FromSubmitted_SetsStatusToWithdrawn()
    {
        var aggregate = RequestAggregate.Submit(NewId("Withdraw"), DefaultDescriptor(), FixedTs);
        aggregate.ClearDomainEvents();
        aggregate.Withdraw();
        Assert.IsType<RequestWithdrawnEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(RequestStatus.Withdrawn, aggregate.Status);
    }

    [Fact]
    public void Approve_AfterApprove_Throws()
    {
        var aggregate = RequestAggregate.Submit(NewId("Approve_Again"), DefaultDescriptor(), FixedTs);
        aggregate.Approve();
        Assert.ThrowsAny<Exception>(() => aggregate.Approve());
    }

    [Fact]
    public void Deny_WithEmptyReason_Throws()
    {
        var aggregate = RequestAggregate.Submit(NewId("Deny_EmptyReason"), DefaultDescriptor(), FixedTs);
        Assert.ThrowsAny<Exception>(() => aggregate.Deny(""));
    }

    [Fact]
    public void Withdraw_AfterApprove_Throws()
    {
        var aggregate = RequestAggregate.Submit(NewId("Withdraw_Approved"), DefaultDescriptor(), FixedTs);
        aggregate.Approve();
        Assert.ThrowsAny<Exception>(() => aggregate.Withdraw());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();
        var history = new object[] { new RequestSubmittedEvent(id, descriptor, FixedTs), new RequestApprovedEvent(id) };
        var aggregate = (RequestAggregate)Activator.CreateInstance(typeof(RequestAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);
        Assert.Equal(id, aggregate.RequestId);
        Assert.Equal(RequestStatus.Approved, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
