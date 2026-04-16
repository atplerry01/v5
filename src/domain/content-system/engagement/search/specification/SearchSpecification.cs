using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Search;

public sealed class SearchSpecification : Specification<SearchIndexStatus>
{
    public override bool IsSatisfiedBy(SearchIndexStatus entity) => entity == SearchIndexStatus.Open;

    public void EnsureOpen(SearchIndexStatus status)
    {
        if (status == SearchIndexStatus.Compacted) throw SearchErrors.CannotMutateCompacted();
    }
}
