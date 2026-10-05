namespace Dijkstra.Core.Domain;

/// <summary>
/// A directed, weighted connection between two points in the network.
/// </summary>
public sealed record Edge(string From, string To, double Weight);
