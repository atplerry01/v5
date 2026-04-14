using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Capital.Vault;
using Whycespace.Shared.Contracts.Economic.Capital.Vault;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Capital.Vault.Application;

public static class CapitalVaultApplicationModule
{
    public static IServiceCollection AddCapitalVaultApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateCapitalVaultHandler>();
        services.AddTransient<AddCapitalVaultSliceHandler>();
        services.AddTransient<DepositToCapitalVaultSliceHandler>();
        services.AddTransient<AllocateFromCapitalVaultSliceHandler>();
        services.AddTransient<ReleaseToCapitalVaultSliceHandler>();
        services.AddTransient<WithdrawFromCapitalVaultSliceHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateCapitalVaultCommand, CreateCapitalVaultHandler>();
        engine.Register<AddCapitalVaultSliceCommand, AddCapitalVaultSliceHandler>();
        engine.Register<DepositToCapitalVaultSliceCommand, DepositToCapitalVaultSliceHandler>();
        engine.Register<AllocateFromCapitalVaultSliceCommand, AllocateFromCapitalVaultSliceHandler>();
        engine.Register<ReleaseToCapitalVaultSliceCommand, ReleaseToCapitalVaultSliceHandler>();
        engine.Register<WithdrawFromCapitalVaultSliceCommand, WithdrawFromCapitalVaultSliceHandler>();
    }
}
