namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed class EntityGraphPathResolver
{
    public EntityExecutionPath ResolveExecutionPath(
        Guid start,
        Guid end,
        IEnumerable<EntityRelationship> relationships)
    {
        var relationshipList = relationships.ToList();
        var queue = new Queue<List<Guid>>();
        var visited = new HashSet<Guid>();

        queue.Enqueue(new List<Guid> { start });

        while (queue.Count > 0)
        {
            var path = queue.Dequeue();
            var node = path[^1];

            if (node == end)
                return new EntityExecutionPath(path);

            if (!visited.Add(node))
                continue;

            var nextNodes = relationshipList
                .Where(r => r.FromEntityId == node)
                .Select(r => r.ToEntityId)
                .OrderBy(x => x);

            foreach (var next in nextNodes)
            {
                var newPath = new List<Guid>(path) { next };
                queue.Enqueue(newPath);
            }
        }

        return new EntityExecutionPath(Array.Empty<Guid>());
    }
}
