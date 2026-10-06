namespace Dijkstra.Core.Algorithms;

/// <summary>
/// Resultado de una búsqueda de ruta: la secuencia ordenada de puntos y el costo total
/// de la ruta óptima, o la indicación de que no existe ninguna ruta.
/// </summary>
public sealed class PathResult
{
    public bool RouteFound { get; }
    public IReadOnlyList<string> Path { get; }
    public double TotalCost { get; }

    private PathResult(bool routeFound, IReadOnlyList<string> path, double totalCost)
    {
        RouteFound = routeFound;
        Path = path;
        TotalCost = totalCost;
    }

    public static PathResult Success(IReadOnlyList<string> path, double totalCost) =>
        new(routeFound: true, path, totalCost);

    public static PathResult NotFound() =>
        new(routeFound: false, Array.Empty<string>(), double.PositiveInfinity);
}
