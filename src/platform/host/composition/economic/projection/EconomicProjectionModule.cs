using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Projections.Economic.Capital.Account;
using Whycespace.Projections.Economic.Capital.Allocation;
using Whycespace.Projections.Economic.Capital.Asset;
using Whycespace.Projections.Economic.Capital.Binding;
using Whycespace.Projections.Economic.Capital.Pool;
using Whycespace.Projections.Economic.Capital.Reserve;
using Whycespace.Projections.Economic.Capital.Vault;
using Whycespace.Projections.Economic.Ledger.Ledger;
using Whycespace.Projections.Economic.Revenue.Distribution;
using Whycespace.Projections.Economic.Revenue.Payout;
using Whycespace.Projections.Economic.Revenue.Revenue;
using Whycespace.Projections.Economic.Vault.Account;
using Whycespace.Projections.Shared;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Economic.Capital.Account;
using Whycespace.Shared.Contracts.Economic.Capital.Allocation;
using Whycespace.Shared.Contracts.Economic.Capital.Asset;
using Whycespace.Shared.Contracts.Economic.Capital.Binding;
using Whycespace.Shared.Contracts.Economic.Capital.Pool;
using Whycespace.Shared.Contracts.Economic.Capital.Reserve;
using Whycespace.Shared.Contracts.Economic.Capital.Vault;
using Whycespace.Shared.Contracts.Economic.Ledger.Ledger;
using Whycespace.Shared.Contracts.Economic.Revenue.Distribution;
using Whycespace.Shared.Contracts.Economic.Revenue.Payout;
using Whycespace.Shared.Contracts.Economic.Revenue.Revenue;
using Whycespace.Shared.Contracts.Economic.Vault.Account;
using Whycespace.Shared.Kernel.Domain;
using PostgresProjectionWriter = Whycespace.Platform.Host.Adapters.PostgresProjectionWriter;

namespace Whycespace.Platform.Host.Composition.Economic.Projection;

/// <summary>
/// Economic projection module — registers four PostgresProjectionStore instances
/// (revenue, distribution, payout, vault/account), four projection handlers,
/// and four GenericKafkaProjectionConsumerWorker instances (one per topic /
/// consumer group).
/// </summary>
public static class EconomicProjectionModule
{
    public static IServiceCollection AddEconomicProjection(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Projection stores ────────────────────────────────────
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<RevenueReadModel>("projection_economic_revenue_revenue", "revenue_read_model", "Revenue"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<DistributionReadModel>("projection_economic_revenue_distribution", "distribution_read_model", "Distribution"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<PayoutReadModel>("projection_economic_revenue_payout", "payout_read_model", "Payout"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<VaultAccountReadModel>("projection_economic_vault_account", "vault_account_read_model", "VaultAccount"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<LedgerReadModel>("projection_economic_ledger_ledger", "ledger_read_model", "Ledger"));

        // Capital projection stores (Phase 2 capital wiring — Option C)
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<CapitalAccountReadModel>("projection_economic_capital_account", "capital_account_read_model", "CapitalAccount"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<CapitalAllocationReadModel>("projection_economic_capital_allocation", "capital_allocation_read_model", "CapitalAllocation"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<CapitalAssetReadModel>("projection_economic_capital_asset", "capital_asset_read_model", "CapitalAsset"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<CapitalBindingReadModel>("projection_economic_capital_binding", "capital_binding_read_model", "CapitalBinding"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<CapitalPoolReadModel>("projection_economic_capital_pool", "capital_pool_read_model", "CapitalPool"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<CapitalReserveReadModel>("projection_economic_capital_reserve", "capital_reserve_read_model", "CapitalReserve"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<CapitalVaultReadModel>("projection_economic_capital_vault", "capital_vault_read_model", "CapitalVault"));

        // ── Projection handlers ──────────────────────────────────
        services.AddSingleton(sp => new RevenueRecordedProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<RevenueReadModel>>()));
        services.AddSingleton(sp => new DistributionCreatedProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<DistributionReadModel>>()));
        services.AddSingleton(sp => new PayoutExecutedProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<PayoutReadModel>>()));
        services.AddSingleton(sp => new VaultAccountProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<VaultAccountReadModel>>()));
        services.AddSingleton(sp => new LedgerUpdatedProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<LedgerReadModel>>()));

        // Capital projection handlers (Phase 2 capital wiring — Option C)
        services.AddSingleton(sp => new CapitalAccountProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<CapitalAccountReadModel>>()));
        services.AddSingleton(sp => new CapitalAllocationProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<CapitalAllocationReadModel>>()));
        services.AddSingleton(sp => new CapitalAssetProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<CapitalAssetReadModel>>()));
        services.AddSingleton(sp => new CapitalBindingProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<CapitalBindingReadModel>>()));
        services.AddSingleton(sp => new CapitalPoolProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<CapitalPoolReadModel>>()));
        services.AddSingleton(sp => new CapitalReserveProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<CapitalReserveReadModel>>()));
        services.AddSingleton(sp => new CapitalVaultProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<CapitalVaultReadModel>>()));

        // ── Kafka projection consumers (one per topic) ───────────
        var kafkaBootstrapServers = configuration.GetValue<string>("Kafka:BootstrapServers")
            ?? throw new InvalidOperationException("Kafka:BootstrapServers is required. No fallback.");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.revenue.revenue.events",
            consumerGroup: "whyce.projection.economic.revenue.revenue",
            projectionSchema: "projection_economic_revenue_revenue",
            projectionTable: "revenue_read_model",
            aggregateType: "Revenue");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.revenue.distribution.events",
            consumerGroup: "whyce.projection.economic.revenue.distribution",
            projectionSchema: "projection_economic_revenue_distribution",
            projectionTable: "distribution_read_model",
            aggregateType: "Distribution");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.revenue.payout.events",
            consumerGroup: "whyce.projection.economic.revenue.payout",
            projectionSchema: "projection_economic_revenue_payout",
            projectionTable: "payout_read_model",
            aggregateType: "Payout");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.vault.account.events",
            consumerGroup: "whyce.projection.economic.vault.account",
            projectionSchema: "projection_economic_vault_account",
            projectionTable: "vault_account_read_model",
            aggregateType: "VaultAccount");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.ledger.journal.events",
            consumerGroup: "whyce.projection.economic.ledger.journal",
            projectionSchema: "projection_economic_ledger_ledger",
            projectionTable: "ledger_read_model",
            aggregateType: "Ledger");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.ledger.ledger.events",
            consumerGroup: "whyce.projection.economic.ledger.ledger",
            projectionSchema: "projection_economic_ledger_ledger",
            projectionTable: "ledger_read_model",
            aggregateType: "Ledger");

        // Capital projection consumers (Phase 2 capital wiring — Option C)
        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.capital.account.events",
            consumerGroup: "whyce.projection.economic.capital.account",
            projectionSchema: "projection_economic_capital_account",
            projectionTable: "capital_account_read_model",
            aggregateType: "CapitalAccount");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.capital.allocation.events",
            consumerGroup: "whyce.projection.economic.capital.allocation",
            projectionSchema: "projection_economic_capital_allocation",
            projectionTable: "capital_allocation_read_model",
            aggregateType: "CapitalAllocation");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.capital.asset.events",
            consumerGroup: "whyce.projection.economic.capital.asset",
            projectionSchema: "projection_economic_capital_asset",
            projectionTable: "capital_asset_read_model",
            aggregateType: "CapitalAsset");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.capital.binding.events",
            consumerGroup: "whyce.projection.economic.capital.binding",
            projectionSchema: "projection_economic_capital_binding",
            projectionTable: "capital_binding_read_model",
            aggregateType: "CapitalBinding");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.capital.pool.events",
            consumerGroup: "whyce.projection.economic.capital.pool",
            projectionSchema: "projection_economic_capital_pool",
            projectionTable: "capital_pool_read_model",
            aggregateType: "CapitalPool");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.capital.reserve.events",
            consumerGroup: "whyce.projection.economic.capital.reserve",
            projectionSchema: "projection_economic_capital_reserve",
            projectionTable: "capital_reserve_read_model",
            aggregateType: "CapitalReserve");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.capital.vault.events",
            consumerGroup: "whyce.projection.economic.capital.vault",
            projectionSchema: "projection_economic_capital_vault",
            projectionTable: "capital_vault_read_model",
            aggregateType: "CapitalVault");

        return services;
    }

    public static void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection)
    {
        var revenueHandler = provider.GetRequiredService<RevenueRecordedProjectionHandler>();
        projection.Register("RevenueRecordedEvent", revenueHandler);

        var distributionHandler = provider.GetRequiredService<DistributionCreatedProjectionHandler>();
        projection.Register("DistributionCreatedEvent", distributionHandler);

        var payoutHandler = provider.GetRequiredService<PayoutExecutedProjectionHandler>();
        projection.Register("PayoutExecutedEvent", payoutHandler);

        var vaultHandler = provider.GetRequiredService<VaultAccountProjectionHandler>();
        projection.Register("VaultFundedEvent", vaultHandler);
        projection.Register("CapitalAllocatedToSliceEvent", vaultHandler);
        projection.Register("SpvProfitReceivedEvent", vaultHandler);
        projection.Register("VaultDebitedEvent", vaultHandler);
        projection.Register("VaultCreditedEvent", vaultHandler);

        var ledgerHandler = provider.GetRequiredService<LedgerUpdatedProjectionHandler>();
        projection.Register("LedgerUpdatedEvent", ledgerHandler);
        projection.Register("JournalEntryAddedEvent", ledgerHandler);

        // Capital projection registrations (Phase 2 capital wiring — Option C)
        var capitalAccountHandler = provider.GetRequiredService<CapitalAccountProjectionHandler>();
        projection.Register("CapitalAccountOpenedEvent", capitalAccountHandler);
        projection.Register("CapitalFundedEvent", capitalAccountHandler);
        projection.Register("AccountCapitalAllocatedEvent", capitalAccountHandler);
        projection.Register("AccountCapitalReservedEvent", capitalAccountHandler);
        projection.Register("AccountReservationReleasedEvent", capitalAccountHandler);
        projection.Register("CapitalAccountFrozenEvent", capitalAccountHandler);
        projection.Register("CapitalAccountClosedEvent", capitalAccountHandler);

        var capitalAllocationHandler = provider.GetRequiredService<CapitalAllocationProjectionHandler>();
        projection.Register("AllocationCreatedEvent", capitalAllocationHandler);
        projection.Register("AllocationReleasedEvent", capitalAllocationHandler);
        projection.Register("AllocationCompletedEvent", capitalAllocationHandler);
        projection.Register("CapitalAllocatedToSpvEvent", capitalAllocationHandler);

        var capitalAssetHandler = provider.GetRequiredService<CapitalAssetProjectionHandler>();
        projection.Register("AssetCreatedEvent", capitalAssetHandler);
        projection.Register("AssetValuedEvent", capitalAssetHandler);
        projection.Register("AssetDisposedEvent", capitalAssetHandler);

        var capitalBindingHandler = provider.GetRequiredService<CapitalBindingProjectionHandler>();
        projection.Register("CapitalBoundEvent", capitalBindingHandler);
        projection.Register("OwnershipTransferredEvent", capitalBindingHandler);
        projection.Register("BindingReleasedEvent", capitalBindingHandler);

        var capitalPoolHandler = provider.GetRequiredService<CapitalPoolProjectionHandler>();
        projection.Register("PoolCreatedEvent", capitalPoolHandler);
        projection.Register("CapitalAggregatedEvent", capitalPoolHandler);
        projection.Register("CapitalReducedEvent", capitalPoolHandler);

        var capitalReserveHandler = provider.GetRequiredService<CapitalReserveProjectionHandler>();
        projection.Register("ReserveCreatedEvent", capitalReserveHandler);
        projection.Register("ReserveReleasedEvent", capitalReserveHandler);
        projection.Register("ReserveExpiredEvent", capitalReserveHandler);

        var capitalVaultHandler = provider.GetRequiredService<CapitalVaultProjectionHandler>();
        projection.Register("VaultCreatedEvent", capitalVaultHandler);
        projection.Register("VaultSliceCreatedEvent", capitalVaultHandler);
        projection.Register("CapitalDepositedEvent", capitalVaultHandler);
        projection.Register("CapitalAllocatedToSliceEvent", capitalVaultHandler);
        projection.Register("CapitalReleasedFromSliceEvent", capitalVaultHandler);
        projection.Register("CapitalWithdrawnEvent", capitalVaultHandler);
    }

    private static void RegisterWorker(
        IServiceCollection services,
        string kafkaBootstrapServers,
        string topic,
        string consumerGroup,
        string projectionSchema,
        string projectionTable,
        string aggregateType)
    {
        services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(sp =>
            new GenericKafkaProjectionConsumerWorker(
                kafkaBootstrapServers,
                topic,
                consumerGroup,
                sp.GetRequiredService<EventDeserializer>(),
                sp.GetRequiredService<ProjectionRegistry>(),
                new PostgresProjectionWriter(
                    sp.GetRequiredService<ProjectionsDataSource>(),
                    projectionSchema,
                    projectionTable,
                    aggregateType),
                sp.GetRequiredService<IClock>(),
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Messaging.KafkaConsumerOptions>(),
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Health.IWorkerLivenessRegistry>(),
                sp.GetService<Microsoft.Extensions.Logging.ILogger<GenericKafkaProjectionConsumerWorker>>()));
    }
}
