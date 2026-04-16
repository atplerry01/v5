using ChargeDomain = Whycespace.Domain.EconomicSystem.Transaction.Charge;
using ChargeSchema = Whycespace.Shared.Contracts.Events.Economic.Transaction.Charge;
using ExpenseDomain = Whycespace.Domain.EconomicSystem.Transaction.Expense;
using ExpenseSchema = Whycespace.Shared.Contracts.Events.Economic.Transaction.Expense;
using InstructionDomain = Whycespace.Domain.EconomicSystem.Transaction.Instruction;
using InstructionSchema = Whycespace.Shared.Contracts.Events.Economic.Transaction.Instruction;
using LimitDomain = Whycespace.Domain.EconomicSystem.Transaction.Limit;
using LimitSchema = Whycespace.Shared.Contracts.Events.Economic.Transaction.Limit;
using SettlementDomain = Whycespace.Domain.EconomicSystem.Transaction.Settlement;
using SettlementSchema = Whycespace.Shared.Contracts.Events.Economic.Transaction.Settlement;
using TransactionDomain = Whycespace.Domain.EconomicSystem.Transaction.Transaction;
using TransactionSchema = Whycespace.Shared.Contracts.Events.Economic.Transaction.Transaction;
using WalletDomain = Whycespace.Domain.EconomicSystem.Transaction.Wallet;
using WalletSchema = Whycespace.Shared.Contracts.Events.Economic.Transaction.Wallet;
using TransactionRefContract = Whycespace.Shared.Contracts.Economic.Transaction.Transaction;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Registers the economic/transaction domain-group event schemas + payload
/// mappers for all seven domains (charge, expense, instruction, limit,
/// settlement, transaction, wallet). Domain events live under
/// Whycespace.Domain.EconomicSystem.Transaction.*; stored schemas live
/// under Whycespace.Shared.Contracts.Events.Economic.Transaction.*.
/// </summary>
public sealed class TransactionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        RegisterCharge(sink);
        RegisterExpense(sink);
        RegisterInstruction(sink);
        RegisterLimit(sink);
        RegisterSettlement(sink);
        RegisterTransaction(sink);
        RegisterWallet(sink);
    }

    // ── Charge ───────────────────────────────────────────────────

    private static void RegisterCharge(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ChargeCalculatedEvent",
            EventVersion.Default,
            typeof(ChargeDomain.ChargeCalculatedEvent),
            typeof(ChargeSchema.ChargeCalculatedEventSchema));
        sink.RegisterPayloadMapper("ChargeCalculatedEvent", e =>
        {
            var evt = (ChargeDomain.ChargeCalculatedEvent)e;
            return new ChargeSchema.ChargeCalculatedEventSchema(
                evt.ChargeId.Value,
                evt.TransactionId,
                evt.Type.ToString(),
                evt.BaseAmount.Value,
                evt.ChargeAmount.Value,
                evt.Currency.Code,
                evt.CalculatedAt.Value);
        });

        sink.RegisterSchema(
            "ChargeAppliedEvent",
            EventVersion.Default,
            typeof(ChargeDomain.ChargeAppliedEvent),
            typeof(ChargeSchema.ChargeAppliedEventSchema));
        sink.RegisterPayloadMapper("ChargeAppliedEvent", e =>
        {
            var evt = (ChargeDomain.ChargeAppliedEvent)e;
            return new ChargeSchema.ChargeAppliedEventSchema(
                evt.ChargeId.Value,
                evt.TransactionId,
                evt.AppliedAmount.Value,
                evt.AppliedAt.Value);
        });
    }

    // ── Expense ──────────────────────────────────────────────────

    private static void RegisterExpense(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ExpenseCreatedEvent",
            EventVersion.Default,
            typeof(ExpenseDomain.ExpenseCreatedEvent),
            typeof(ExpenseSchema.ExpenseCreatedEventSchema));
        sink.RegisterPayloadMapper("ExpenseCreatedEvent", e =>
        {
            var evt = (ExpenseDomain.ExpenseCreatedEvent)e;
            return new ExpenseSchema.ExpenseCreatedEventSchema(
                Guid.Parse(evt.ExpenseId),
                evt.Amount,
                evt.Currency,
                evt.Category,
                evt.SourceReference);
        });

        sink.RegisterSchema(
            "ExpenseRecordedEvent",
            EventVersion.Default,
            typeof(ExpenseDomain.ExpenseRecordedEvent),
            typeof(ExpenseSchema.ExpenseRecordedEventSchema));
        sink.RegisterPayloadMapper("ExpenseRecordedEvent", e =>
        {
            var evt = (ExpenseDomain.ExpenseRecordedEvent)e;
            return new ExpenseSchema.ExpenseRecordedEventSchema(
                Guid.Parse(evt.ExpenseId),
                evt.Amount,
                evt.Currency);
        });

        sink.RegisterSchema(
            "ExpenseCancelledEvent",
            EventVersion.Default,
            typeof(ExpenseDomain.ExpenseCancelledEvent),
            typeof(ExpenseSchema.ExpenseCancelledEventSchema));
        sink.RegisterPayloadMapper("ExpenseCancelledEvent", e =>
        {
            var evt = (ExpenseDomain.ExpenseCancelledEvent)e;
            return new ExpenseSchema.ExpenseCancelledEventSchema(
                Guid.Parse(evt.ExpenseId),
                evt.Reason);
        });
    }

    // ── Instruction ──────────────────────────────────────────────

    private static void RegisterInstruction(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "TransactionInstructionCreatedEvent",
            EventVersion.Default,
            typeof(InstructionDomain.TransactionInstructionCreatedEvent),
            typeof(InstructionSchema.TransactionInstructionCreatedEventSchema));
        sink.RegisterPayloadMapper("TransactionInstructionCreatedEvent", e =>
        {
            var evt = (InstructionDomain.TransactionInstructionCreatedEvent)e;
            return new InstructionSchema.TransactionInstructionCreatedEventSchema(
                evt.InstructionId.Value,
                evt.FromAccountId,
                evt.ToAccountId,
                evt.Amount.Value,
                evt.Currency.Code,
                evt.Type.ToString(),
                evt.CreatedAt.Value);
        });

        sink.RegisterSchema(
            "TransactionInstructionExecutedEvent",
            EventVersion.Default,
            typeof(InstructionDomain.TransactionInstructionExecutedEvent),
            typeof(InstructionSchema.TransactionInstructionExecutedEventSchema));
        sink.RegisterPayloadMapper("TransactionInstructionExecutedEvent", e =>
        {
            var evt = (InstructionDomain.TransactionInstructionExecutedEvent)e;
            return new InstructionSchema.TransactionInstructionExecutedEventSchema(
                evt.InstructionId.Value,
                evt.ExecutedAt.Value);
        });

        sink.RegisterSchema(
            "TransactionInstructionCancelledEvent",
            EventVersion.Default,
            typeof(InstructionDomain.TransactionInstructionCancelledEvent),
            typeof(InstructionSchema.TransactionInstructionCancelledEventSchema));
        sink.RegisterPayloadMapper("TransactionInstructionCancelledEvent", e =>
        {
            var evt = (InstructionDomain.TransactionInstructionCancelledEvent)e;
            return new InstructionSchema.TransactionInstructionCancelledEventSchema(
                evt.InstructionId.Value,
                evt.Reason,
                evt.CancelledAt.Value);
        });
    }

    // ── Limit ────────────────────────────────────────────────────

    private static void RegisterLimit(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "LimitDefinedEvent",
            EventVersion.Default,
            typeof(LimitDomain.LimitDefinedEvent),
            typeof(LimitSchema.LimitDefinedEventSchema));
        sink.RegisterPayloadMapper("LimitDefinedEvent", e =>
        {
            var evt = (LimitDomain.LimitDefinedEvent)e;
            return new LimitSchema.LimitDefinedEventSchema(
                evt.LimitId.Value,
                evt.AccountId,
                evt.Type.ToString(),
                evt.Threshold.Value,
                evt.Currency.Code,
                evt.DefinedAt.Value);
        });

        sink.RegisterSchema(
            "LimitCheckedEvent",
            EventVersion.Default,
            typeof(LimitDomain.LimitCheckedEvent),
            typeof(LimitSchema.LimitCheckedEventSchema));
        sink.RegisterPayloadMapper("LimitCheckedEvent", e =>
        {
            var evt = (LimitDomain.LimitCheckedEvent)e;
            return new LimitSchema.LimitCheckedEventSchema(
                evt.LimitId.Value,
                evt.TransactionId,
                evt.TransactionAmount.Value,
                evt.CurrentUtilization.Value,
                evt.CheckedAt.Value);
        });

        sink.RegisterSchema(
            "LimitExceededEvent",
            EventVersion.Default,
            typeof(LimitDomain.LimitExceededEvent),
            typeof(LimitSchema.LimitExceededEventSchema));
        sink.RegisterPayloadMapper("LimitExceededEvent", e =>
        {
            var evt = (LimitDomain.LimitExceededEvent)e;
            return new LimitSchema.LimitExceededEventSchema(
                evt.LimitId.Value,
                evt.TransactionId,
                evt.AttemptedAmount.Value,
                evt.Threshold.Value,
                evt.ExceededAt.Value);
        });
    }

    // ── Settlement ───────────────────────────────────────────────

    private static void RegisterSettlement(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "SettlementInitiatedEvent",
            EventVersion.Default,
            typeof(SettlementDomain.SettlementInitiatedEvent),
            typeof(SettlementSchema.SettlementInitiatedEventSchema));
        sink.RegisterPayloadMapper("SettlementInitiatedEvent", e =>
        {
            var evt = (SettlementDomain.SettlementInitiatedEvent)e;
            return new SettlementSchema.SettlementInitiatedEventSchema(
                Guid.Parse(evt.SettlementId),
                evt.Amount,
                evt.Currency,
                evt.SourceReference,
                evt.Provider);
        });

        sink.RegisterSchema(
            "SettlementProcessingStartedEvent",
            EventVersion.Default,
            typeof(SettlementDomain.SettlementProcessingStartedEvent),
            typeof(SettlementSchema.SettlementProcessingStartedEventSchema));
        sink.RegisterPayloadMapper("SettlementProcessingStartedEvent", e =>
        {
            var evt = (SettlementDomain.SettlementProcessingStartedEvent)e;
            return new SettlementSchema.SettlementProcessingStartedEventSchema(
                Guid.Parse(evt.SettlementId));
        });

        sink.RegisterSchema(
            "SettlementCompletedEvent",
            EventVersion.Default,
            typeof(SettlementDomain.SettlementCompletedEvent),
            typeof(SettlementSchema.SettlementCompletedEventSchema));
        sink.RegisterPayloadMapper("SettlementCompletedEvent", e =>
        {
            var evt = (SettlementDomain.SettlementCompletedEvent)e;
            return new SettlementSchema.SettlementCompletedEventSchema(
                Guid.Parse(evt.SettlementId),
                evt.ExternalReferenceId);
        });

        sink.RegisterSchema(
            "SettlementFailedEvent",
            EventVersion.Default,
            typeof(SettlementDomain.SettlementFailedEvent),
            typeof(SettlementSchema.SettlementFailedEventSchema));
        sink.RegisterPayloadMapper("SettlementFailedEvent", e =>
        {
            var evt = (SettlementDomain.SettlementFailedEvent)e;
            return new SettlementSchema.SettlementFailedEventSchema(
                Guid.Parse(evt.SettlementId),
                evt.Reason);
        });
    }

    // ── Transaction ──────────────────────────────────────────────

    private static void RegisterTransaction(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "TransactionInitiatedEvent",
            EventVersion.Default,
            typeof(TransactionDomain.TransactionInitiatedEvent),
            typeof(TransactionSchema.TransactionInitiatedEventSchema));
        sink.RegisterPayloadMapper("TransactionInitiatedEvent", e =>
        {
            var evt = (TransactionDomain.TransactionInitiatedEvent)e;
            return new TransactionSchema.TransactionInitiatedEventSchema(
                evt.TransactionId.Value,
                evt.Kind,
                MapReferences(evt.References),
                evt.InitiatedAt.Value);
        });

        sink.RegisterSchema(
            "TransactionProcessingStartedEvent",
            EventVersion.Default,
            typeof(TransactionDomain.TransactionProcessingStartedEvent),
            typeof(TransactionSchema.TransactionInitiatedEventSchema));
        // no dedicated schema for processing-started; emit initiated shape is
        // intentional — downstream projections treat the two states uniformly.
        sink.RegisterPayloadMapper("TransactionProcessingStartedEvent", e =>
        {
            var evt = (TransactionDomain.TransactionProcessingStartedEvent)e;
            return new TransactionSchema.TransactionInitiatedEventSchema(
                evt.TransactionId.Value,
                string.Empty,
                Array.Empty<TransactionRefContract.TransactionReferenceDto>(),
                evt.ProcessingStartedAt.Value);
        });

        sink.RegisterSchema(
            "TransactionCommittedEvent",
            EventVersion.Default,
            typeof(TransactionDomain.TransactionCommittedEvent),
            typeof(TransactionSchema.TransactionCommittedEventSchema));
        sink.RegisterPayloadMapper("TransactionCommittedEvent", e =>
        {
            var evt = (TransactionDomain.TransactionCommittedEvent)e;
            return new TransactionSchema.TransactionCommittedEventSchema(
                evt.TransactionId.Value,
                evt.Kind,
                MapReferences(evt.References),
                evt.CommittedAt.Value);
        });

        sink.RegisterSchema(
            "TransactionFailedEvent",
            EventVersion.Default,
            typeof(TransactionDomain.TransactionFailedEvent),
            typeof(TransactionSchema.TransactionFailedEventSchema));
        sink.RegisterPayloadMapper("TransactionFailedEvent", e =>
        {
            var evt = (TransactionDomain.TransactionFailedEvent)e;
            return new TransactionSchema.TransactionFailedEventSchema(
                evt.TransactionId.Value,
                evt.Reason,
                evt.FailedAt.Value);
        });
    }

    // ── Wallet ───────────────────────────────────────────────────

    private static void RegisterWallet(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "WalletCreatedEvent",
            EventVersion.Default,
            typeof(WalletDomain.WalletCreatedEvent),
            typeof(WalletSchema.WalletCreatedEventSchema));
        sink.RegisterPayloadMapper("WalletCreatedEvent", e =>
        {
            var evt = (WalletDomain.WalletCreatedEvent)e;
            return new WalletSchema.WalletCreatedEventSchema(
                evt.WalletId.Value,
                evt.OwnerId,
                evt.AccountId,
                evt.CreatedAt.Value);
        });

        sink.RegisterSchema(
            "TransactionRequestedEvent",
            EventVersion.Default,
            typeof(WalletDomain.TransactionRequestedEvent),
            typeof(WalletSchema.TransactionRequestedEventSchema));
        sink.RegisterPayloadMapper("TransactionRequestedEvent", e =>
        {
            var evt = (WalletDomain.TransactionRequestedEvent)e;
            return new WalletSchema.TransactionRequestedEventSchema(
                evt.WalletId.Value,
                evt.AccountId,
                evt.DestinationAccountId,
                evt.Amount.Value,
                evt.Currency.Code,
                evt.RequestedAt.Value);
        });
    }

    private static IReadOnlyList<TransactionRefContract.TransactionReferenceDto> MapReferences(
        IReadOnlyList<TransactionDomain.TransactionReference> refs)
    {
        var dtos = new List<TransactionRefContract.TransactionReferenceDto>(refs.Count);
        foreach (var r in refs) dtos.Add(new TransactionRefContract.TransactionReferenceDto(r.Kind, r.Id));
        return dtos;
    }
}
