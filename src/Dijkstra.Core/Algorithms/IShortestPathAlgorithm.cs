using Dijkstra.Core.Domain;

namespace Dijkstra.Core.Algorithms;

/// <summary>
/// Calcula la ruta de menor costo entre dos puntos de un <see cref="IGraph"/>.
/// Las implementaciones no deben depender del tamaño ni de la estructura del grafo.
/// </summary>
public interface IShortestPathAlgorithm
{
    PathResult FindShortestPath(IGraph graph, string startNodeId, string destinationNodeId);
}
