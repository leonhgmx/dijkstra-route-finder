using Dijkstra.Core.Algorithms;
using Dijkstra.Core.Domain;
using Xunit;

namespace Dijkstra.Tests;

public class DijkstraAlgorithmTests
{
    private readonly DijkstraAlgorithm _algorithm = new();

    [Fact]
    public void FindShortestPath_SimpleGraph_ReturnsKnownOptimalRoute()
    {
        // A -> B -> C costs 2, the direct A -> C edge costs 5.
        var graph = new Graph();
        graph.AddEdge("A", "B", 1);
        graph.AddEdge("B", "C", 1);
        graph.AddEdge("A", "C", 5);

        var result = _algorithm.FindShortestPath(graph, "A", "C");

        Assert.True(result.RouteFound);
        Assert.Equal(new[] { "A", "B", "C" }, result.Path);
        Assert.Equal(2, result.TotalCost);
    }

    [Fact]
    public void FindShortestPath_DirectConnectionIsNotOptimal_PrefersCheaperDetour()
    {
        var graph = new Graph();
        graph.AddEdge("A", "B", 10); // direct, expensive
        graph.AddEdge("A", "C", 1);
        graph.AddEdge("C", "B", 1); // detour, cheaper overall

        var result = _algorithm.FindShortestPath(graph, "A", "B");

        Assert.True(result.RouteFound);
        Assert.Equal(new[] { "A", "C", "B" }, result.Path);
        Assert.Equal(2, result.TotalCost);
    }

    [Fact]
    public void FindShortestPath_MultipleEqualCostRoutes_ReturnsCorrectMinimumCost()
    {
        var graph = new Graph();
        graph.AddEdge("A", "B", 1);
        graph.AddEdge("A", "C", 1);
        graph.AddEdge("B", "D", 1);
        graph.AddEdge("C", "D", 1);

        var result = _algorithm.FindShortestPath(graph, "A", "D");

        Assert.True(result.RouteFound);
        Assert.Equal(2, result.TotalCost);
        AssertPathCostMatches(graph, result);
        Assert.Equal("A", result.Path[0]);
        Assert.Equal("D", result.Path[^1]);
        Assert.Equal(3, result.Path.Count);
    }

    [Fact]
    public void FindShortestPath_NoRouteExists_ReturnsNotFound()
    {
        var graph = new Graph();
        graph.AddNode("A"); // isolated point, no outgoing connections
        graph.AddNode("C");
        graph.AddEdge("C", "B", 1); // B is only reachable from C, not from A

        var result = _algorithm.FindShortestPath(graph, "A", "B");

        Assert.False(result.RouteFound);
        Assert.Empty(result.Path);
        Assert.True(double.IsPositiveInfinity(result.TotalCost));
    }

    [Fact]
    public void FindShortestPath_SinglePointGraph_StartEqualsDestination()
    {
        var graph = new Graph();
        graph.AddNode("Only");

        var result = _algorithm.FindShortestPath(graph, "Only", "Only");

        Assert.True(result.RouteFound);
        Assert.Equal(new[] { "Only" }, result.Path);
        Assert.Equal(0, result.TotalCost);
    }

    [Fact]
    public void FindShortestPath_UnknownStartNode_Throws()
    {
        var graph = new Graph();
        graph.AddNode("A");

        Assert.Throws<ArgumentException>(() => _algorithm.FindShortestPath(graph, "missing", "A"));
    }

    [Fact]
    public void FindShortestPath_UnknownDestinationNode_Throws()
    {
        var graph = new Graph();
        graph.AddNode("A");

        Assert.Throws<ArgumentException>(() => _algorithm.FindShortestPath(graph, "A", "missing"));
    }

    [Fact]
    public void FindShortestPath_DifferentWeights_PicksLowestTotalCost()
    {
        var graph = new Graph();
        graph.AddEdge("A", "B", 7);
        graph.AddEdge("B", "C", 2);
        graph.AddEdge("A", "C", 20);
        graph.AddEdge("C", "D", 1);
        graph.AddEdge("B", "D", 10);

        var result = _algorithm.FindShortestPath(graph, "A", "D");

        Assert.True(result.RouteFound);
        Assert.Equal(new[] { "A", "B", "C", "D" }, result.Path);
        Assert.Equal(10, result.TotalCost);
        AssertPathCostMatches(graph, result);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public void FindShortestPath_ScalesAcrossDifferentNetworkSizes(int nodeCount)
    {
        // A chain of `nodeCount` points, each connected to the next with weight 1.
        var graph = new Graph();
        for (var i = 0; i < nodeCount - 1; i++)
        {
            graph.AddEdge($"P{i}", $"P{i + 1}", 1);
        }

        if (nodeCount == 1)
        {
            graph.AddNode("P0");
        }

        var start = "P0";
        var destination = $"P{nodeCount - 1}";

        var result = _algorithm.FindShortestPath(graph, start, destination);

        Assert.True(result.RouteFound);
        Assert.Equal(nodeCount - 1, result.TotalCost);
        Assert.Equal(nodeCount, result.Path.Count);
        AssertPathCostMatches(graph, result);
    }

    [Fact]
    public void FindShortestPath_ReturnedPathIsVerifiablyOptimal_AgainstBruteForce()
    {
        var graph = new Graph();
        graph.AddEdge("A", "B", 4, bidirectional: true);
        graph.AddEdge("A", "C", 2, bidirectional: true);
        graph.AddEdge("C", "B", 1, bidirectional: true);
        graph.AddEdge("B", "D", 5, bidirectional: true);
        graph.AddEdge("C", "D", 8, bidirectional: true);
        graph.AddEdge("C", "E", 10, bidirectional: true);
        graph.AddEdge("D", "E", 2, bidirectional: true);
        graph.AddEdge("D", "F", 6, bidirectional: true);
        graph.AddEdge("E", "F", 3, bidirectional: true);

        var result = _algorithm.FindShortestPath(graph, "A", "F");
        var bruteForceCost = BruteForceShortestCost(graph, "A", "F");

        Assert.True(result.RouteFound);
        Assert.Equal(bruteForceCost, result.TotalCost);
        AssertPathCostMatches(graph, result);
    }

    private static void AssertPathCostMatches(IGraph graph, PathResult result)
    {
        double accumulated = 0;
        for (var i = 0; i < result.Path.Count - 1; i++)
        {
            var from = result.Path[i];
            var to = result.Path[i + 1];
            var edge = graph.GetOutgoingEdges(from).First(e => e.To == to);
            accumulated += edge.Weight;
        }

        Assert.Equal(result.TotalCost, accumulated, precision: 6);
    }

    /// <summary>Exhaustive DFS search used only to independently verify Dijkstra's result in tests.</summary>
    private static double BruteForceShortestCost(IGraph graph, string start, string destination)
    {
        var best = double.PositiveInfinity;
        Visit(start, 0, new HashSet<string> { start });
        return best;

        void Visit(string current, double costSoFar, HashSet<string> visited)
        {
            if (current == destination)
            {
                best = Math.Min(best, costSoFar);
                return;
            }

            foreach (var edge in graph.GetOutgoingEdges(current))
            {
                if (visited.Contains(edge.To) || costSoFar + edge.Weight >= best)
                {
                    continue;
                }

                visited.Add(edge.To);
                Visit(edge.To, costSoFar + edge.Weight, visited);
                visited.Remove(edge.To);
            }
        }
    }
}
