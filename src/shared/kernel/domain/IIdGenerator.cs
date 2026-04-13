namespace Whycespace.Shared.Kernel.Domain;

public interface IIdGenerator
{
    Guid Generate(string seed);
}
