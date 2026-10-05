namespace Dijkstra.Core.Algorithms;

/// <summary>
/// Outcome of a route-finding query: either the ordered sequence of points and the
/// total cost of the optimal route, or an indication that no route exists.
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
