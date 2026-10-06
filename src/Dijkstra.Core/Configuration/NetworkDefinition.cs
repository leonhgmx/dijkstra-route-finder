namespace Dijkstra.Core.Configuration;

/// <summary>
/// Estructura de datos simple que describe una red: sus puntos y las conexiones entre
/// ellos. Es lo único que cambia para soportar una red de 5, 50 o 1000 puntos — el
/// algoritmo y la implementación del grafo nunca necesitan cambiar junto con ella.
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
