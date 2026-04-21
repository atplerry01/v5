using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Customer.SegmentationAndLifecycle.ContactPoint;
using Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.ContactPoint;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Customer.SegmentationAndLifecycle.ContactPoint.Application;

public static class ContactPointApplicationModule
{
    public static IServiceCollection AddContactPointApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateContactPointHandler>();
        services.AddTransient<UpdateContactPointHandler>();
        services.AddTransient<ActivateContactPointHandler>();
        services.AddTransient<SetContactPointPreferredHandler>();
        services.AddTransient<ArchiveContactPointHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateContactPointCommand, CreateContactPointHandler>();
        engine.Register<UpdateContactPointCommand, UpdateContactPointHandler>();
        engine.Register<ActivateContactPointCommand, ActivateContactPointHandler>();
        engine.Register<SetContactPointPreferredCommand, SetContactPointPreferredHandler>();
        engine.Register<ArchiveContactPointCommand, ArchiveContactPointHandler>();
    }
}
