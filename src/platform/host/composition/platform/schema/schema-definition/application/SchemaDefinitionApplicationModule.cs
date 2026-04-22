using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Platform.Schema.SchemaDefinition;
using Whycespace.Shared.Contracts.Platform.Schema.SchemaDefinition;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Platform.Schema.SchemaDefinition.Application;

public static class SchemaDefinitionApplicationModule
{
    public static IServiceCollection AddSchemaDefinitionApplication(this IServiceCollection services)
    {
        services.AddTransient<DraftSchemaDefinitionHandler>();
        services.AddTransient<PublishSchemaDefinitionHandler>();
        services.AddTransient<DeprecateSchemaDefinitionHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DraftSchemaDefinitionCommand, DraftSchemaDefinitionHandler>();
        engine.Register<PublishSchemaDefinitionCommand, PublishSchemaDefinitionHandler>();
        engine.Register<DeprecateSchemaDefinitionCommand, DeprecateSchemaDefinitionHandler>();
    }
}
