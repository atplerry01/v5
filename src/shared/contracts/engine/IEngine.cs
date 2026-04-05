namespace Whyce.Shared.Contracts.Engine;

public interface IEngine
{
    Task ExecuteAsync(IEngineContext context);
}
