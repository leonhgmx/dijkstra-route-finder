using Dijkstra.Core.Domain;

namespace Dijkstra.Core.Configuration;

/// <summary>
/// Construye un <see cref="IGraph"/> a partir de una <see cref="NetworkDefinition"/>.
/// Es el único lugar que traduce la configuración externa al modelo de dominio.
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
