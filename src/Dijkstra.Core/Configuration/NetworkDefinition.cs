namespace Dijkstra.Core.Configuration;

/// <summary>
/// Plain data shape describing a network: its points and the connections between them.
/// This is the only thing that changes to support a network of 5, 50, or 1000 points —
/// the algorithm and graph implementation never need to change alongside it.
/// </summary>
public sealed class NetworkDefinition
{
    public List<string> Nodes { get; set; } = new();
    public List<ConnectionDefinition> Connections { get; set; } = new();
}

public sealed class ConnectionDefinition
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public double Weight { get; set; }
    public bool Bidirectional { get; set; }
}
