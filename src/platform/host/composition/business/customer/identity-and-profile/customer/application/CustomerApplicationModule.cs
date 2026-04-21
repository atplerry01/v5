using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Customer.IdentityAndProfile.Customer;
using Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Customer;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Customer.IdentityAndProfile.Customer.Application;

public static class CustomerApplicationModule
{
    public static IServiceCollection AddCustomerApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateCustomerHandler>();
        services.AddTransient<RenameCustomerHandler>();
        services.AddTransient<ReclassifyCustomerHandler>();
        services.AddTransient<ActivateCustomerHandler>();
        services.AddTransient<ArchiveCustomerHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateCustomerCommand, CreateCustomerHandler>();
        engine.Register<RenameCustomerCommand, RenameCustomerHandler>();
        engine.Register<ReclassifyCustomerCommand, ReclassifyCustomerHandler>();
        engine.Register<ActivateCustomerCommand, ActivateCustomerHandler>();
        engine.Register<ArchiveCustomerCommand, ArchiveCustomerHandler>();
    }
}
