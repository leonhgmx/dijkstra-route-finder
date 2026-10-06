using System.Text.Json;
using Dijkstra.Core.Algorithms;
using Dijkstra.Core.Configuration;
using Dijkstra.Core.Domain;

var configPath = args.Length > 0 ? args[0] : Path.Combine(AppContext.BaseDirectory, "network.sample.json");

if (!File.Exists(configPath))
{
    Console.Error.WriteLine($"No se encontró el archivo de configuración de la red: {configPath}");
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
    Console.Error.WriteLine($"No se pudo cargar la configuración de la red: {ex.Message}");
    return 1;
}

Console.WriteLine($"Red cargada con {graph.NodeIds.Count} punto(s) desde '{Path.GetFileName(configPath)}'.");

var start = args.Length > 1 ? args[1] : PromptForNode(graph, "punto de origen");
var destination = args.Length > 2 ? args[2] : PromptForNode(graph, "punto de destino");

IShortestPathAlgorithm algorithm = new DijkstraAlgorithm();

try
{
    var result = algorithm.FindShortestPath(graph, start, destination);

    if (!result.RouteFound)
    {
        Console.WriteLine($"No existe ninguna ruta entre '{start}' y '{destination}'.");
        return 2;
    }

    Console.WriteLine($"Ruta óptima: {string.Join(" -> ", result.Path)}");
    Console.WriteLine($"Costo total: {result.TotalCost}");
    return 0;
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

static string PromptForNode(IGraph graph, string label)
{
    Console.WriteLine($"Puntos disponibles: {string.Join(", ", graph.NodeIds)}");
    Console.Write($"Ingresa el {label}: ");
    return Console.ReadLine()?.Trim() ?? string.Empty;
}
