using Microsoft.Extensions.DependencyInjection;
using Whycespace.Shared.Contracts.Economic.Transaction.Charge;
using Whycespace.Shared.Contracts.Economic.Transaction.Expense;
using Whycespace.Shared.Contracts.Economic.Transaction.Instruction;
using Whycespace.Shared.Contracts.Economic.Transaction.Limit;
using Whycespace.Shared.Contracts.Economic.Transaction.Settlement;
using Whycespace.Shared.Contracts.Economic.Transaction.Transaction;
using Whycespace.Shared.Contracts.Economic.Transaction.Wallet;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Economic.Transaction;

/// <summary>
/// E11 — transaction context policy bindings. Registers one
/// <see cref="CommandPolicyBinding"/> per transaction command, mapping the
/// command CLR type to its canonical policy id constant. The bindings are
/// aggregated by <see cref="ICommandPolicyIdRegistry"/> at runtime; once
/// registered, every dispatch of a transaction command stamps the correct
/// policy id onto <c>CommandContext.PolicyId</c> for evaluation by
/// <c>PolicyMiddleware</c>.
///
/// Coverage: 17 commands across 7 subdomains (transaction, instruction,
/// settlement, charge, limit, wallet, expense).
/// </summary>
public static class TransactionPolicyModule
{
    public static IServiceCollection AddTransactionPolicyBindings(this IServiceCollection services)
    {
        // ── transaction envelope (3) ──────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(InitiateTransactionCommand), TransactionPolicyIds.Initiate));
        services.AddSingleton(new CommandPolicyBinding(typeof(CommitTransactionCommand),   TransactionPolicyIds.Commit));
        services.AddSingleton(new CommandPolicyBinding(typeof(FailTransactionCommand),     TransactionPolicyIds.Fail));

        // ── instruction (3) ───────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateInstructionCommand),  InstructionPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(ExecuteInstructionCommand), InstructionPolicyIds.Execute));
        services.AddSingleton(new CommandPolicyBinding(typeof(CancelInstructionCommand),  InstructionPolicyIds.Cancel));

        // ── settlement (3) ────────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(InitiateSettlementCommand), SettlementPolicyIds.Initiate));
        services.AddSingleton(new CommandPolicyBinding(typeof(CompleteSettlementCommand), SettlementPolicyIds.Complete));
        services.AddSingleton(new CommandPolicyBinding(typeof(FailSettlementCommand),     SettlementPolicyIds.Fail));

        // ── charge (2) ───────────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CalculateChargeCommand), ChargePolicyIds.Calculate));
        services.AddSingleton(new CommandPolicyBinding(typeof(ApplyChargeCommand),     ChargePolicyIds.Apply));

        // ── limit (2) ────────────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(DefineLimitCommand), LimitPolicyIds.Define));
        services.AddSingleton(new CommandPolicyBinding(typeof(CheckLimitCommand),  LimitPolicyIds.Check));

        // ── wallet (2) ───────────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateWalletCommand),             WalletPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(RequestWalletTransactionCommand), WalletPolicyIds.RequestTransaction));

        // ── expense (1) ──────────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(RecordExpenseCommand), ExpensePolicyIds.Record));

        return services;
    }
}
