using Whyce.Shared.Contracts.Engine;

namespace Whyce.Runtime.Dispatcher;

public sealed class EngineRegistry : IEngineRegistry
{
    private readonly Dictionary<Type, Type> _registrations = new();

    public void Register<TCommand, TEngine>() where TEngine : IEngine
    {
        _registrations[typeof(TCommand)] = typeof(TEngine);
    }

    public Type? ResolveEngine(Type commandType)
    {
        return _registrations.GetValueOrDefault(commandType);
    }
}
