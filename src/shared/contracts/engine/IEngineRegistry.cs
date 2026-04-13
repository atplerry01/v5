namespace Whycespace.Shared.Contracts.Engine;

public interface IEngineRegistry
{
    void Register<TCommand, TEngine>() where TEngine : IEngine;
    Type? ResolveEngine(Type commandType);
}
