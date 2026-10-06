namespace Dijkstra.Core.Domain;

/// <summary>
/// Grafo mutable por lista de adyacencia. La cantidad de puntos y conexiones depende
/// enteramente de lo que se agregue en tiempo de ejecución, por lo que el tamaño de la
/// red nunca queda fijo en el código.
/// </summary>
public sealed class Graph : IGraph
{
    private readonly Dictionary<string, List<Edge>> _adjacency = new();

    public IReadOnlyCollection<string> NodeIds => _adjacency.Keys;

    public void AddNode(string nodeId)
    {
        ValidateNodeId(nodeId);
        if (!_adjacency.ContainsKey(nodeId))
        {
            _adjacency[nodeId] = new List<Edge>();
        }
    }

    /// <summary>
    /// Agrega una conexión con peso desde <paramref name="fromNodeId"/> hacia
    /// <paramref name="toNodeId"/>. Ambos extremos se crean automáticamente si no
    /// existen todavía.
    /// </summary>
    public void AddEdge(string fromNodeId, string toNodeId, double weight, bool bidirectional = false)
    {
        ValidateNodeId(fromNodeId);
        ValidateNodeId(toNodeId);
        if (weight < 0 || double.IsNaN(weight))
        {
            throw new ArgumentOutOfRangeException(nameof(weight), weight,
                "El algoritmo de Dijkstra requiere pesos de conexión no negativos.");
        }

        AddNode(fromNodeId);
        AddNode(toNodeId);

        _adjacency[fromNodeId].Add(new Edge(fromNodeId, toNodeId, weight));
        if (bidirectional)
        {
            _adjacency[toNodeId].Add(new Edge(toNodeId, fromNodeId, weight));
        }
    }

    public bool ContainsNode(string nodeId) => _adjacency.ContainsKey(nodeId);

    public IEnumerable<Edge> GetOutgoingEdges(string nodeId) =>
        _adjacency.TryGetValue(nodeId, out var edges) ? edges : Enumerable.Empty<Edge>();

    private static void ValidateNodeId(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            throw new ArgumentException("El id de un punto no puede ser nulo ni estar vacío.", nameof(nodeId));
        }
    }
}
