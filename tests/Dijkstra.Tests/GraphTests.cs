using Dijkstra.Core.Domain;
using Xunit;

namespace Dijkstra.Tests;

public class GraphTests
{
    [Fact]
    public void AddNode_IsIdempotent()
    {
        var graph = new Graph();

        graph.AddNode("A");
        graph.AddNode("A");

        Assert.Single(graph.NodeIds);
    }

    [Fact]
    public void AddEdge_AutoCreatesMissingEndpoints()
    {
        var graph = new Graph();

        graph.AddEdge("A", "B", 3);

        Assert.True(graph.ContainsNode("A"));
        Assert.True(graph.ContainsNode("B"));
    }

    [Fact]
    public void AddEdge_Bidirectional_CreatesConnectionInBothDirections()
    {
        var graph = new Graph();

        graph.AddEdge("A", "B", 3, bidirectional: true);

        Assert.Single(graph.GetOutgoingEdges("A"), e => e.To == "B" && e.Weight == 3);
        Assert.Single(graph.GetOutgoingEdges("B"), e => e.To == "A" && e.Weight == 3);
    }

    [Fact]
    public void AddEdge_Directed_OnlyCreatesOneDirection()
    {
        var graph = new Graph();

        graph.AddEdge("A", "B", 3);

        Assert.Single(graph.GetOutgoingEdges("A"));
        Assert.Empty(graph.GetOutgoingEdges("B"));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.001)]
    public void AddEdge_NegativeWeight_Throws(double weight)
    {
        var graph = new Graph();

        Assert.Throws<ArgumentOutOfRangeException>(() => graph.AddEdge("A", "B", weight));
    }

    [Fact]
    public void GetOutgoingEdges_UnknownNode_ReturnsEmpty()
    {
        var graph = new Graph();

        Assert.Empty(graph.GetOutgoingEdges("does-not-exist"));
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public void Graph_SupportsArbitraryNumberOfPoints(int pointCount)
    {
        var graph = new Graph();

        for (var i = 0; i < pointCount; i++)
        {
            graph.AddNode($"P{i}");
        }

        Assert.Equal(pointCount, graph.NodeIds.Count);
    }
}
