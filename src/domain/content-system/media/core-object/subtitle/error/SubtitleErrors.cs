using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Subtitle;

public static class SubtitleErrors
{
    public static DomainException SubtitleFinalized()
        => new("Subtitle is finalized and cannot be modified.");

    public static DomainException SubtitleArchived()
        => new("Cannot mutate an archived subtitle.");

    public static DomainException AlreadyArchived()
        => new("Subtitle is already archived.");

    public static DomainException AlreadyFinalized()
        => new("Subtitle is already finalized.");

    public static DomainInvariantViolationException OrphanedSubtitle()
        => new("Subtitle must reference an owning media asset.");
}
