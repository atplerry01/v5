using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Subject.Subject;
using Whycespace.Shared.Contracts.Economic.Subject.Subject;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Subject.Subject.Application;

public static class SubjectApplicationModule
{
    public static IServiceCollection AddSubjectApplication(this IServiceCollection services)
    {
        services.AddTransient<RegisterEconomicSubjectHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RegisterEconomicSubjectCommand, RegisterEconomicSubjectHandler>();
    }
}
