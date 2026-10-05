namespace Dijkstra.Core.Domain;

/// <summary>
/// Read-only view of a network of points and connections. Route-finding algorithms
/// depend only on this abstraction, never on how the graph is built or stored.
/// </summary>
public interface IGraph
{
    /// <summary>Every point currently known to the graph.</summary>
    IReadOnlyCollection<string> NodeIds { get; }

    /// <summary>Whether a point with the given id exists in the graph.</summary>
    bool ContainsNode(string nodeId);

    /// <summary>All connections leaving the given point. Empty if the point has none.</summary>
    IEnumerable<Edge> GetOutgoingEdges(string nodeId);
}
