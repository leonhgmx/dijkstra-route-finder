using System.Text.Json;

namespace Dijkstra.Core.Configuration;

/// <summary>
/// Reads a <see cref="NetworkDefinition"/> from JSON, so the network a user wants to
/// model (however many points it has) can be supplied externally, not hardcoded.
/// </summary>
public static class NetworkConfigLoader
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static NetworkDefinition LoadFromFile(string filePath)
    {
        var json = File.ReadAllText(filePath);
        return LoadFromJson(json);
    }

    public static NetworkDefinition LoadFromJson(string json)
    {
        var definition = JsonSerializer.Deserialize<NetworkDefinition>(json, Options);
        return definition ?? throw new InvalidOperationException("The network configuration could not be parsed.");
    }
}
