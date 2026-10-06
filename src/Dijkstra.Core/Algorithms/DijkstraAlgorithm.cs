using Dijkstra.Core.Domain;

namespace Dijkstra.Core.Algorithms;

/// <summary>
/// Algoritmo clásico de Dijkstra apoyado en una cola de prioridad con heap binario.
/// Se ejecuta en O((V + E) log V) y trabaja únicamente contra la abstracción
/// <see cref="IGraph"/>, por lo que es independiente del tamaño y la estructura de la red.
/// </summary>
public sealed class DijkstraAlgorithm : IShortestPathAlgorithm
{
    public PathResult FindShortestPath(IGraph graph, string startNodeId, string destinationNodeId)
    {
        ArgumentNullException.ThrowIfNull(graph);

        if (!graph.ContainsNode(startNodeId))
        {
            throw new ArgumentException($"El punto de origen '{startNodeId}' no existe en la red.", nameof(startNodeId));
        }

        if (!graph.ContainsNode(destinationNodeId))
        {
            throw new ArgumentException($"El punto de destino '{destinationNodeId}' no existe en la red.", nameof(destinationNodeId));
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
