using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Search;

public static class SearchErrors
{
    public static DomainException InvalidDocument() => new("Search document reference must be non-empty.");
    public static DomainException InvalidIndexName() => new("Search index name must be non-empty.");
    public static DomainException DocumentAlreadyIndexed(string ref_) => new($"Document '{ref_}' already indexed.");
    public static DomainException DocumentNotIndexed(string ref_) => new($"Document '{ref_}' not indexed.");
    public static DomainException CannotMutateCompacted() => new("Compacted indexes are immutable.");
    public static DomainInvariantViolationException NameMissing() =>
        new("Invariant violated: search index must have a name.");
}
