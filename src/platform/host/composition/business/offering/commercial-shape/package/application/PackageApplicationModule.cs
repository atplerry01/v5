using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Offering.CommercialShape.Package;
using Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Package;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Offering.CommercialShape.Package.Application;

public static class PackageApplicationModule
{
    public static IServiceCollection AddPackageApplication(this IServiceCollection services)
    {
        services.AddTransient<CreatePackageHandler>();
        services.AddTransient<AddPackageMemberHandler>();
        services.AddTransient<RemovePackageMemberHandler>();
        services.AddTransient<ActivatePackageHandler>();
        services.AddTransient<ArchivePackageHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreatePackageCommand, CreatePackageHandler>();
        engine.Register<AddPackageMemberCommand, AddPackageMemberHandler>();
        engine.Register<RemovePackageMemberCommand, RemovePackageMemberHandler>();
        engine.Register<ActivatePackageCommand, ActivatePackageHandler>();
        engine.Register<ArchivePackageCommand, ArchivePackageHandler>();
    }
}
