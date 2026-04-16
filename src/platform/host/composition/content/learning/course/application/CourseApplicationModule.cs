using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Learning.Course;
using Whycespace.Shared.Contracts.Content.Learning.Course;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Learning.Course.Application;

public static class CourseApplicationModule
{
    public static IServiceCollection AddCourseApplication(this IServiceCollection services)
    {
        services.AddTransient<DraftCourseHandler>();
        services.AddTransient<AttachCourseModuleHandler>();
        services.AddTransient<DetachCourseModuleHandler>();
        services.AddTransient<PublishCourseHandler>();
        services.AddTransient<ArchiveCourseHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DraftCourseCommand, DraftCourseHandler>();
        engine.Register<AttachCourseModuleCommand, AttachCourseModuleHandler>();
        engine.Register<DetachCourseModuleCommand, DetachCourseModuleHandler>();
        engine.Register<PublishCourseCommand, PublishCourseHandler>();
        engine.Register<ArchiveCourseCommand, ArchiveCourseHandler>();
    }
}
