using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Command.CommandCatalog;

public static class CommandCatalogErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("CommandCatalog has already been initialized.");
}
