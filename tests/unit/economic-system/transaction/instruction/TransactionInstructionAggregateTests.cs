using Whycespace.Domain.EconomicSystem.Transaction.Instruction;
using Whycespace.Domain.EconomicSystem.Capital.Account;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Transaction.Instruction;

public sealed class TransactionInstructionAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp Later = new(new DateTimeOffset(2026, 4, 22, 11, 0, 0, TimeSpan.Zero));
    private static readonly Currency Usd = new("USD");

    private static InstructionId NewId(string seed) =>
        new(IdGen.Generate($"TxInstructionTests:{seed}:instruction"));

    private static AccountId NewAccountId(string seed) =>
        new(IdGen.Generate($"TxInstructionTests:{seed}:account"));

    [Fact]
    public void CreateInstruction_RaisesCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var from = NewAccountId("Create_from");
        var to = NewAccountId("Create_to");

        var aggregate = TransactionInstructionAggregate.CreateInstruction(
            id, from, to, new Amount(1000m), Usd, InstructionType.Transfer, BaseTime);

        var evt = Assert.IsType<TransactionInstructionCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.InstructionId);
        Assert.Equal(1000m, evt.Amount.Value);
        Assert.Equal(InstructionType.Transfer, evt.Type);
    }

    [Fact]
    public void CreateInstruction_SetsStateFromEvent()
    {
        var id = NewId("Create_State");
        var from = NewAccountId("State_from");
        var to = NewAccountId("State_to");

        var aggregate = TransactionInstructionAggregate.CreateInstruction(
            id, from, to, new Amount(500m), Usd, InstructionType.Payment, BaseTime);

        Assert.Equal(id, aggregate.InstructionId);
        Assert.Equal(InstructionStatus.Pending, aggregate.Status);
        Assert.Equal(500m, aggregate.Amount.Value);
    }

    [Fact]
    public void CreateInstruction_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var from = NewAccountId("Stable_from");
        var to = NewAccountId("Stable_to");

        var a1 = TransactionInstructionAggregate.CreateInstruction(id, from, to, new Amount(100m), Usd, InstructionType.Transfer, BaseTime);
        var a2 = TransactionInstructionAggregate.CreateInstruction(id, from, to, new Amount(100m), Usd, InstructionType.Transfer, BaseTime);

        Assert.Equal(
            ((TransactionInstructionCreatedEvent)a1.DomainEvents[0]).InstructionId.Value,
            ((TransactionInstructionCreatedEvent)a2.DomainEvents[0]).InstructionId.Value);
    }

    [Fact]
    public void CreateInstruction_ZeroAmount_Throws()
    {
        var id = NewId("Zero");
        var from = NewAccountId("Zero_from");
        var to = NewAccountId("Zero_to");

        Assert.ThrowsAny<Exception>(() =>
            TransactionInstructionAggregate.CreateInstruction(id, from, to, new Amount(0m), Usd, InstructionType.Transfer, BaseTime));
    }

    [Fact]
    public void CreateInstruction_SameAccounts_Throws()
    {
        var id = NewId("SameAccounts");
        var account = NewAccountId("SameAccounts_both");

        Assert.ThrowsAny<Exception>(() =>
            TransactionInstructionAggregate.CreateInstruction(id, account, account, new Amount(100m), Usd, InstructionType.Transfer, BaseTime));
    }

    [Fact]
    public void MarkExecuted_FromPending_RaisesExecutedEvent()
    {
        var id = NewId("Execute");
        var aggregate = TransactionInstructionAggregate.CreateInstruction(
            id, NewAccountId("Execute_from"), NewAccountId("Execute_to"),
            new Amount(200m), Usd, InstructionType.Transfer, BaseTime);
        aggregate.ClearDomainEvents();

        aggregate.MarkExecuted(Later);

        Assert.IsType<TransactionInstructionExecutedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(InstructionStatus.Executed, aggregate.Status);
    }

    [Fact]
    public void CancelInstruction_FromPending_RaisesCancelledEvent()
    {
        var id = NewId("Cancel");
        var aggregate = TransactionInstructionAggregate.CreateInstruction(
            id, NewAccountId("Cancel_from"), NewAccountId("Cancel_to"),
            new Amount(300m), Usd, InstructionType.Transfer, BaseTime);
        aggregate.ClearDomainEvents();

        aggregate.CancelInstruction("User requested cancellation.", Later);

        Assert.IsType<TransactionInstructionCancelledEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(InstructionStatus.Cancelled, aggregate.Status);
    }

    [Fact]
    public void MarkExecuted_AfterCancelled_Throws()
    {
        var id = NewId("ExecuteAfterCancel");
        var aggregate = TransactionInstructionAggregate.CreateInstruction(
            id, NewAccountId("EAC_from"), NewAccountId("EAC_to"),
            new Amount(400m), Usd, InstructionType.Transfer, BaseTime);
        aggregate.CancelInstruction("Cancelled first.", BaseTime);

        Assert.ThrowsAny<Exception>(() => aggregate.MarkExecuted(Later));
    }

    [Fact]
    public void LoadFromHistory_RehydratesPendingState()
    {
        var id = NewId("History");
        var from = NewAccountId("History_from");
        var to = NewAccountId("History_to");

        var history = new object[]
        {
            new TransactionInstructionCreatedEvent(id, from.Value, to.Value, new Amount(600m), Usd, InstructionType.Allocation, BaseTime)
        };

        var aggregate = (TransactionInstructionAggregate)Activator.CreateInstance(typeof(TransactionInstructionAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.InstructionId);
        Assert.Equal(600m, aggregate.Amount.Value);
        Assert.Equal(InstructionStatus.Pending, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
