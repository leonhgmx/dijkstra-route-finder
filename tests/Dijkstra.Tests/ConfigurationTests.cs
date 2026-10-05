using Dijkstra.Core.Algorithms;
using Dijkstra.Core.Configuration;
using Xunit;

namespace Dijkstra.Tests;

public class ConfigurationTests
{
    [Fact]
    public void GraphFactory_BuildsGraphMatchingDefinition()
    {
        var definition = new NetworkDefinition
        {
            Nodes = new List<string> { "A", "B", "C" },
            Connections = new List<ConnectionDefinition>
            {
                new() { From = "A", To = "B", Weight = 2, Bidirectional = true },
                new() { From = "B", To = "C", Weight = 3 },
            },
        };

        var graph = GraphFactory.CreateFromDefinition(definition);

        Assert.Equal(3, graph.NodeIds.Count);
        Assert.Contains(graph.GetOutgoingEdges("A"), e => e.To == "B" && e.Weight == 2);
        Assert.Contains(graph.GetOutgoingEdges("B"), e => e.To == "A" && e.Weight == 2);
        Assert.Contains(graph.GetOutgoingEdges("B"), e => e.To == "C" && e.Weight == 3);
        Assert.Empty(graph.GetOutgoingEdges("C"));
    }

    [Fact]
    public void NetworkConfigLoader_ParsesJsonIntoDefinition()
    {
        const string json = """
        {
          "nodes": ["A", "B", "C"],
          "connections": [
            { "from": "A", "to": "B", "weight": 1, "bidirectional": true },
            { "from": "B", "to": "C", "weight": 4 }
          ]
        }
        """;

        var definition = NetworkConfigLoader.LoadFromJson(json);

        Assert.Equal(new[] { "A", "B", "C" }, definition.Nodes);
        Assert.Equal(2, definition.Connections.Count);
        Assert.Equal(4, definition.Connections[1].Weight);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(25)]
    [InlineData(100)]
    public void GraphFactory_SupportsConfigurableNetworkSize_WithoutAlgorithmChanges(int pointCount)
    {
        var definition = new NetworkDefinition();
        for (var i = 0; i < pointCount; i++)
        {
            definition.Nodes.Add($"P{i}");
        }

        for (var i = 0; i < pointCount - 1; i++)
        {
            definition.Connections.Add(new ConnectionDefinition { From = $"P{i}", To = $"P{i + 1}", Weight = 1 });
        }

        var graph = GraphFactory.CreateFromDefinition(definition);
        var result = new DijkstraAlgorithm().FindShortestPath(graph, "P0", $"P{pointCount - 1}");

        Assert.Equal(pointCount, graph.NodeIds.Count);
        Assert.True(result.RouteFound);
        Assert.Equal(pointCount - 1, result.TotalCost);
    }

    [Fact]
    public void NetworkConfigLoader_LoadFromFile_ReadsNetworkFromDisk()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, """
            {
              "nodes": ["X", "Y"],
              "connections": [ { "from": "X", "to": "Y", "weight": 9 } ]
            }
            """);

            var definition = NetworkConfigLoader.LoadFromFile(tempFile);

            Assert.Equal(new[] { "X", "Y" }, definition.Nodes);
            Assert.Equal(9, definition.Connections.Single().Weight);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
