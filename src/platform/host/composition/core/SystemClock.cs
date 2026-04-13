using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Composition.Core;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
