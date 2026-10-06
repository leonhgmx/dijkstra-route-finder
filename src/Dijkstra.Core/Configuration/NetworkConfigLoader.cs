using System.Text.Json;

namespace Dijkstra.Core.Configuration;

/// <summary>
/// Lee una <see cref="NetworkDefinition"/> desde JSON, de modo que la red que el
/// usuario quiere modelar (sin importar cuántos puntos tenga) pueda suministrarse
/// externamente en lugar de quedar fija en el código.
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
        return definition ?? throw new InvalidOperationException("No se pudo interpretar la configuración de la red.");
    }
}
