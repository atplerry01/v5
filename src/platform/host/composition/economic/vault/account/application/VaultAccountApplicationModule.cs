using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Vault.Account;
using Whycespace.Shared.Contracts.Economic.Vault.Account;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Vault.Account.Application;

/// <summary>
/// Vault Account application module — T2E command handler DI registrations
/// and engine bindings for ApplyRevenue / DebitSlice / CreditSlice.
/// </summary>
public static class VaultAccountApplicationModule
{
    public static IServiceCollection AddVaultAccountApplication(this IServiceCollection services)
    {
        services.AddTransient<ApplyRevenueHandler>();
        services.AddTransient<DebitSliceHandler>();
        services.AddTransient<CreditSliceHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<ApplyRevenueCommand, ApplyRevenueHandler>();
        engine.Register<DebitSliceCommand, DebitSliceHandler>();
        engine.Register<CreditSliceCommand, CreditSliceHandler>();
    }
}
