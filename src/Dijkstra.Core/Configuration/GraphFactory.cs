using Dijkstra.Core.Domain;

namespace Dijkstra.Core.Configuration;

/// <summary>
/// Builds an <see cref="IGraph"/> from a <see cref="NetworkDefinition"/>. This is the
/// single place that translates external configuration into the domain model.
/// </summary>
public static class GraphFactory
{
    public static IGraph CreateFromDefinition(NetworkDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var graph = new Graph();

        foreach (var nodeId in definition.Nodes)
        {
            graph.AddNode(nodeId);
        }

        foreach (var connection in definition.Connections)
        {
            graph.AddEdge(connection.From, connection.To, connection.Weight, connection.Bidirectional);
        }

        return graph;
    }
}
