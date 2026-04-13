namespace Whycespace.Engines.T0U.Determinism.Sequence;

/// <summary>
/// HSID v2.1 sequence resolver. Returns the next bounded sequence value
/// (0..0xFFF for the locked X3 width) within a given scope. Scope is the
/// composite "topology:seed" key — sequences reset per scope.
///
/// Implementations are async because the canonical backend
/// (<c>PersistedSequenceResolver</c> over <c>ISequenceStore</c>) is IO-bound.
/// </summary>
public interface ISequenceResolver
{
    Task<int> NextAsync(string scope);
}
