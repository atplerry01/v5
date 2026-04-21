using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Command.CommandCatalog;

public sealed class CommandCatalogAggregate : AggregateRoot
{
    public static CommandCatalogAggregate Create()
    {
        var aggregate = new CommandCatalogAggregate();
        if (aggregate.Version >= 0)
            throw CommandCatalogErrors.AlreadyInitialized();

        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }
}
