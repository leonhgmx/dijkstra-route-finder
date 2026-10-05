namespace Dijkstra.Core.Domain;

/// <summary>
/// Mutable adjacency-list graph. The number of points and connections is entirely
/// driven by what is added at runtime, so network size is never hardcoded.
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
    /// Adds a weighted connection from <paramref name="fromNodeId"/> to <paramref name="toNodeId"/>.
    /// Both endpoints are created automatically if they do not already exist.
    /// </summary>
    public void AddEdge(string fromNodeId, string toNodeId, double weight, bool bidirectional = false)
    {
        ValidateNodeId(fromNodeId);
        ValidateNodeId(toNodeId);
        if (weight < 0 || double.IsNaN(weight))
        {
            throw new ArgumentOutOfRangeException(nameof(weight), weight,
                "Dijkstra's algorithm requires non-negative connection weights.");
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
            throw new ArgumentException("A point id must not be null or empty.", nameof(nodeId));
        }
    }
}
