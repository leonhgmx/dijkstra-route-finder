using System.Text.Json;
using Dijkstra.Core.Algorithms;
using Dijkstra.Core.Configuration;
using Dijkstra.Core.Domain;

var configPath = args.Length > 0 ? args[0] : Path.Combine(AppContext.BaseDirectory, "network.sample.json");

if (!File.Exists(configPath))
{
    Console.Error.WriteLine($"Network configuration file not found: {configPath}");
    return 1;
}

NetworkDefinition definition;
IGraph graph;
try
{
    definition = NetworkConfigLoader.LoadFromFile(configPath);
    graph = GraphFactory.CreateFromDefinition(definition);
}
catch (Exception ex) when (ex is JsonException or InvalidOperationException or ArgumentException)
{
    Console.Error.WriteLine($"Could not load network configuration: {ex.Message}");
    return 1;
}

Console.WriteLine($"Loaded network with {graph.NodeIds.Count} point(s) from '{Path.GetFileName(configPath)}'.");

var start = args.Length > 1 ? args[1] : PromptForNode(graph, "starting point");
var destination = args.Length > 2 ? args[2] : PromptForNode(graph, "destination point");

IShortestPathAlgorithm algorithm = new DijkstraAlgorithm();

try
{
    var result = algorithm.FindShortestPath(graph, start, destination);

    if (!result.RouteFound)
    {
        Console.WriteLine($"No route exists between '{start}' and '{destination}'.");
        return 2;
    }

    Console.WriteLine($"Optimal route: {string.Join(" -> ", result.Path)}");
    Console.WriteLine($"Total cost: {result.TotalCost}");
    return 0;
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

static string PromptForNode(IGraph graph, string label)
{
    Console.WriteLine($"Available points: {string.Join(", ", graph.NodeIds)}");
    Console.Write($"Enter {label}: ");
    return Console.ReadLine()?.Trim() ?? string.Empty;
}
