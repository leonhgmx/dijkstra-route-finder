namespace Dijkstra.Core.Domain;

/// <summary>
/// Una conexión dirigida y con peso entre dos puntos de la red.
/// </summary>
public sealed record Edge(string From, string To, double Weight);
