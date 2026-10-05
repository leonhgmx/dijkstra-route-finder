using Dijkstra.Core.Domain;

namespace Dijkstra.Core.Algorithms;

/// <summary>
/// Classic Dijkstra shortest-path algorithm backed by a binary-heap priority queue.
/// Runs in O((V + E) log V) and works against the <see cref="IGraph"/> abstraction only,
/// so it is agnostic to network size and structure.
/// </summary>
public sealed class DijkstraAlgorithm : IShortestPathAlgorithm
{
    public PathResult FindShortestPath(IGraph graph, string startNodeId, string destinationNodeId)
    {
        ArgumentNullException.ThrowIfNull(graph);

        if (!graph.ContainsNode(startNodeId))
        {
            throw new ArgumentException($"Start point '{startNodeId}' does not exist in the network.", nameof(startNodeId));
        }

        if (!graph.ContainsNode(destinationNodeId))
        {
            throw new ArgumentException($"Destination point '{destinationNodeId}' does not exist in the network.", nameof(destinationNodeId));
        }

        var distances = new Dictionary<string, double> { [startNodeId] = 0 };
        var previous = new Dictionary<string, string>();
        var visited = new HashSet<string>();
        var frontier = new PriorityQueue<string, double>();
        frontier.Enqueue(startNodeId, 0);

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            if (!visited.Add(current))
            {
                continue;
            }

            if (current == destinationNodeId)
            {
                break;
            }

            var currentDistance = distances[current];
            foreach (var edge in graph.GetOutgoingEdges(current))
            {
                if (visited.Contains(edge.To))
                {
                    continue;
                }

                var candidateDistance = currentDistance + edge.Weight;
                if (!distances.TryGetValue(edge.To, out var knownDistance) || candidateDistance < knownDistance)
                {
                    distances[edge.To] = candidateDistance;
                    previous[edge.To] = current;
                    frontier.Enqueue(edge.To, candidateDistance);
                }
            }
        }

        if (!distances.TryGetValue(destinationNodeId, out var totalCost))
        {
            return PathResult.NotFound();
        }

        return PathResult.Success(BuildPath(previous, startNodeId, destinationNodeId), totalCost);
    }

    private static IReadOnlyList<string> BuildPath(IReadOnlyDictionary<string, string> previous, string startNodeId, string destinationNodeId)
    {
        var path = new List<string> { destinationNodeId };
        var current = destinationNodeId;

        while (current != startNodeId)
        {
            current = previous[current];
            path.Add(current);
        }

        path.Reverse();
        return path;
    }
}
