using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Transaction.Charge;
using Whycespace.Engines.T2E.Economic.Transaction.Expense;
using Whycespace.Engines.T2E.Economic.Transaction.Instruction;
using Whycespace.Engines.T2E.Economic.Transaction.Limit;
using Whycespace.Engines.T2E.Economic.Transaction.Settlement;
using Whycespace.Engines.T2E.Economic.Transaction.Transaction;
using Whycespace.Engines.T2E.Economic.Transaction.Wallet;
using Whycespace.Shared.Contracts.Economic.Transaction.Charge;
using Whycespace.Shared.Contracts.Economic.Transaction.Expense;
using Whycespace.Shared.Contracts.Economic.Transaction.Instruction;
using Whycespace.Shared.Contracts.Economic.Transaction.Limit;
using Whycespace.Shared.Contracts.Economic.Transaction.Settlement;
using Whycespace.Shared.Contracts.Economic.Transaction.Transaction;
using Whycespace.Shared.Contracts.Economic.Transaction.Wallet;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Transaction;

/// <summary>
/// Application wiring for the economic/transaction domain group (charge,
/// expense, instruction, limit, settlement, transaction, wallet). Registers
/// every T2E command handler in DI and binds each command to its handler in
/// the engine registry.
/// </summary>
public static class TransactionApplicationModule
{
    public static IServiceCollection AddTransactionApplication(this IServiceCollection services)
    {
        // Charge
        services.AddTransient<CalculateChargeHandler>();
        services.AddTransient<ApplyChargeHandler>();

        // Expense
        services.AddTransient<RecordExpenseHandler>();

        // Instruction
        services.AddTransient<CreateInstructionHandler>();
        services.AddTransient<ExecuteInstructionHandler>();
        services.AddTransient<CancelInstructionHandler>();

        // Limit
        services.AddTransient<DefineLimitHandler>();
        services.AddTransient<CheckLimitHandler>();

        // Settlement
        services.AddTransient<InitiateSettlementHandler>();
        services.AddTransient<CompleteSettlementHandler>();
        services.AddTransient<FailSettlementHandler>();

        // Transaction envelope
        services.AddTransient<InitiateTransactionHandler>();
        services.AddTransient<CommitTransactionHandler>();
        services.AddTransient<FailTransactionHandler>();

        // Wallet
        services.AddTransient<CreateWalletHandler>();
        services.AddTransient<RequestWalletTransactionHandler>();

        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CalculateChargeCommand, CalculateChargeHandler>();
        engine.Register<ApplyChargeCommand, ApplyChargeHandler>();

        engine.Register<RecordExpenseCommand, RecordExpenseHandler>();

        engine.Register<CreateInstructionCommand, CreateInstructionHandler>();
        engine.Register<ExecuteInstructionCommand, ExecuteInstructionHandler>();
        engine.Register<CancelInstructionCommand, CancelInstructionHandler>();

        engine.Register<DefineLimitCommand, DefineLimitHandler>();
        engine.Register<CheckLimitCommand, CheckLimitHandler>();

        engine.Register<InitiateSettlementCommand, InitiateSettlementHandler>();
        engine.Register<CompleteSettlementCommand, CompleteSettlementHandler>();
        engine.Register<FailSettlementCommand, FailSettlementHandler>();

        engine.Register<InitiateTransactionCommand, InitiateTransactionHandler>();
        engine.Register<CommitTransactionCommand, CommitTransactionHandler>();
        engine.Register<FailTransactionCommand, FailTransactionHandler>();

        engine.Register<CreateWalletCommand, CreateWalletHandler>();
        engine.Register<RequestWalletTransactionCommand, RequestWalletTransactionHandler>();
    }
}
