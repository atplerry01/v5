using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Streaming.StreamCore.Channel;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Channel;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Streaming.StreamCore.Channel.Application;

public static class ChannelApplicationModule
{
    public static IServiceCollection AddChannelApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateChannelHandler>();
        services.AddTransient<RenameChannelHandler>();
        services.AddTransient<EnableChannelHandler>();
        services.AddTransient<DisableChannelHandler>();
        services.AddTransient<ArchiveChannelHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateChannelCommand, CreateChannelHandler>();
        engine.Register<RenameChannelCommand, RenameChannelHandler>();
        engine.Register<EnableChannelCommand, EnableChannelHandler>();
        engine.Register<DisableChannelCommand, DisableChannelHandler>();
        engine.Register<ArchiveChannelCommand, ArchiveChannelHandler>();
    }
}
