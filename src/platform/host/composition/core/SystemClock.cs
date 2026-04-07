using Whyce.Shared.Kernel.Domain;

namespace Whyce.Platform.Host.Composition.Core;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
