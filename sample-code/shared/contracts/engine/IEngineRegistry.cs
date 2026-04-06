namespace Whycespace.Shared.Contracts.Engine;

public interface IEngineRegistry
{
    void Register(string commandType, IEngine engine);
    IEngine? Resolve(string commandType);
    bool HasEngine(string commandType);
}
