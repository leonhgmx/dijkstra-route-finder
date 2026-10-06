namespace Dijkstra.Core.Domain;

/// <summary>
/// Vista de solo lectura de una red de puntos y conexiones. Los algoritmos de búsqueda
/// de rutas dependen únicamente de esta abstracción, nunca de cómo se construye o
/// almacena el grafo.
/// </summary>
public interface IGraph
{
    /// <summary>Todos los puntos actualmente conocidos por el grafo.</summary>
    IReadOnlyCollection<string> NodeIds { get; }

    /// <summary>Indica si existe un punto con el id dado en el grafo.</summary>
    bool ContainsNode(string nodeId);

    /// <summary>Todas las conexiones que salen del punto dado. Vacío si no tiene ninguna.</summary>
    IEnumerable<Edge> GetOutgoingEdges(string nodeId);
}
