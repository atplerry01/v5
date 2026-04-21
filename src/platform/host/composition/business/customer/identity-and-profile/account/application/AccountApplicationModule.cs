using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Customer.IdentityAndProfile.Account;
using Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Account;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Customer.IdentityAndProfile.Account.Application;

public static class AccountApplicationModule
{
    public static IServiceCollection AddAccountApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateAccountHandler>();
        services.AddTransient<RenameAccountHandler>();
        services.AddTransient<ActivateAccountHandler>();
        services.AddTransient<SuspendAccountHandler>();
        services.AddTransient<CloseAccountHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateAccountCommand, CreateAccountHandler>();
        engine.Register<RenameAccountCommand, RenameAccountHandler>();
        engine.Register<ActivateAccountCommand, ActivateAccountHandler>();
        engine.Register<SuspendAccountCommand, SuspendAccountHandler>();
        engine.Register<CloseAccountCommand, CloseAccountHandler>();
    }
}
