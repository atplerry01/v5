using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.Control.Access;

public readonly record struct AccessWindow
{
    public Timestamp Start { get; }
    public Timestamp End { get; }

    public AccessWindow(Timestamp start, Timestamp end)
    {
        Guard.Against(end.Value <= start.Value, "AccessWindow end must be after start.");
        Start = start;
        End = end;
    }

    public bool Includes(Timestamp instant) =>
        instant.Value >= Start.Value && instant.Value <= End.Value;

    public bool HasExpired(Timestamp now) => now.Value > End.Value;
}
