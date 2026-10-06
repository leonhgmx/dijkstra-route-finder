# Dijkstra — Buscador de Ruta Óptima

Implementación en C# del algoritmo de Dijkstra para encontrar la ruta de menor costo
sobre una red configurable de puntos y conexiones con peso.

## Estructura de la solución

```
Dijkstra.sln
src/
  Dijkstra.Core/            Biblioteca de clases — toda la lógica que importa
    Domain/                 Puntos, conexiones y la abstracción del grafo
      IGraph.cs             Contrato de solo lectura del que depende el algoritmo
      Graph.cs              Implementación por lista de adyacencia (el tamaño nunca está fijo en el código)
      Edge.cs                Una conexión dirigida con peso
    Algorithms/             Cálculo de rutas
      IShortestPathAlgorithm.cs
      DijkstraAlgorithm.cs  El algoritmo en sí — depende únicamente de IGraph
      PathResult.cs         Ruta + costo total, o "no existe ruta"
    Configuration/          Convierte definiciones externas de red en un grafo
      NetworkDefinition.cs  DTOs simples (puntos + conexiones)
      NetworkConfigLoader.cs  Lee una NetworkDefinition desde JSON
      GraphFactory.cs       NetworkDefinition -> IGraph
  Dijkstra.App/              Capa de presentación por consola
    Program.cs               Carga una red, pregunta origen/destino, imprime la ruta
    network.sample.json      Red de ejemplo con 6 puntos
  Dijkstra.Desktop/          Capa de presentación WPF — visualizador gráfico
    MainWindow.xaml(.cs)     Dibuja puntos/conexiones en un canvas, resalta la ruta óptima
                             y expone controles para parametrizar la red en tiempo de ejecución
tests/
  Dijkstra.Tests/            Suite de pruebas xUnit (30 pruebas)
```

## Notas de diseño

- **El algoritmo nunca sabe qué tan grande es la red ni cómo fue construida.**
  `DijkstraAlgorithm` depende únicamente de `IGraph`, una interfaz que expone los
  identificadores de los puntos y las conexiones salientes. Ya sea que la red tenga
  5 puntos o 5000, construida desde código, desde JSON o desde una base de datos, el
  algoritmo no se ve afectado.
- **El tamaño y la estructura de la red son configuración externa, no código.**
  `NetworkDefinition` + `NetworkConfigLoader` permiten describir una red de cualquier
  tamaño en un archivo JSON y convertirla en un grafo mediante `GraphFactory`, sin
  modificar el modelo de dominio ni el algoritmo.
- **La separación de responsabilidades** sigue las capas pedidas en el enunciado:
  representación del grafo (`Domain`), cálculo de rutas (`Algorithms`), configuración
  (`Configuration`) y presentación — dos capas de presentación independientes
  (`Dijkstra.App` por consola, `Dijkstra.Desktop` por WPF) se apoyan sobre la misma
  biblioteca `Dijkstra.Core` sin que esta sepa que alguna de las dos existe.
- **Complejidad:** `DijkstraAlgorithm` usa una cola de prioridad con heap binario
  (`PriorityQueue<string, double>`), lo que da un tiempo de `O((V + E) log V)`, la cota
  estándar para Dijkstra con heap binario.
- **Casos límite manejados:** ausencia de ruta entre dos puntos, origen igual a destino,
  redes de un solo punto, puntos de origen/destino desconocidos (lanza
  `ArgumentException`), y rechazo de pesos de conexión negativos (precondición de
  Dijkstra).

## Ejecutar la aplicación de consola

```bash
dotnet run --project src/Dijkstra.App -- src/Dijkstra.App/network.sample.json A F
```

Omite los argumentos de origen/destino para que se pidan de forma interactiva; omite la
ruta de configuración para usar el `network.sample.json` incluido.

## Ejecutar el visualizador de escritorio

```bash
dotnet run --project src/Dijkstra.Desktop
```

Una muestra gráfica e independiente de la misma lógica de `Dijkstra.Core` (solo Windows —
WPF). Dibuja los puntos y conexiones de la red en un canvas y resalta la ruta óptima
calculada (verde = origen, rojo = destino, naranja = parte de la ruta). La red es
completamente parametrizable desde la interfaz:

- **Generar una red aleatoria** — ingresa una cantidad de puntos y haz clic en *Generar*
  para construir una red conexa aleatoria de ese tamaño (árbol de expansión + conexiones
  extra aleatorias).
- **Construirla manualmente** — agrega puntos individuales y luego conexiones entre ellos
  con un peso y una dirección configurables (bidireccional o no).
- **Cargar la red de ejemplo** — carga la misma red de 6 puntos que usa la app de consola.
- Elige un **origen** y un **destino** entre los puntos actuales y haz clic en *Calcular
  ruta óptima* para ejecutar el algoritmo de Dijkstra y ver el resultado tanto en texto
  (ruta + costo total, o "no existe ruta") como resaltado en el canvas.

## Ejecutar las pruebas

```bash
dotnet test
```

La suite cubre: un grafo simple con una ruta óptima conocida, un grafo donde la conexión
directa no es la óptima, múltiples rutas posibles, un grafo sin ruta entre origen y
destino, un grafo de un solo punto, distintos tamaños de red (5/10/50/100 puntos),
conexiones con distintos pesos, la corrección del costo total calculado, y que la ruta
devuelta sea efectivamente óptima (verificado de forma cruzada contra una búsqueda por
fuerza bruta).
