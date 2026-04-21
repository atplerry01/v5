using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Customer.SegmentationAndLifecycle.Segment;
using Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.Segment;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Customer.SegmentationAndLifecycle.Segment.Application;

public static class SegmentApplicationModule
{
    public static IServiceCollection AddSegmentApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateSegmentHandler>();
        services.AddTransient<UpdateSegmentHandler>();
        services.AddTransient<ActivateSegmentHandler>();
        services.AddTransient<ArchiveSegmentHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateSegmentCommand, CreateSegmentHandler>();
        engine.Register<UpdateSegmentCommand, UpdateSegmentHandler>();
        engine.Register<ActivateSegmentCommand, ActivateSegmentHandler>();
        engine.Register<ArchiveSegmentCommand, ArchiveSegmentHandler>();
    }
}
