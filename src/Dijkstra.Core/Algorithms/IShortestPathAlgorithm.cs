using Dijkstra.Core.Domain;

namespace Dijkstra.Core.Algorithms;

/// <summary>
/// Computes the lowest-cost route between two points of an <see cref="IGraph"/>.
/// Implementations must not depend on the size or structure of the graph.
/// </summary>
public interface IShortestPathAlgorithm
{
    PathResult FindShortestPath(IGraph graph, string startNodeId, string destinationNodeId);
}
