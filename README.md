# Dijkstra — Optimal Route Finder

A C# implementation of Dijkstra's shortest-path algorithm over a configurable network
of points and weighted connections.

## Solution layout

```
Dijkstra.sln
src/
  Dijkstra.Core/            Class library — all the logic that matters
    Domain/                 Points, connections, and the graph abstraction
      IGraph.cs             Read-only contract the algorithm depends on
      Graph.cs              Adjacency-list implementation (size is never hardcoded)
      Edge.cs                A weighted, directed connection
    Algorithms/             Route calculation
      IShortestPathAlgorithm.cs
      DijkstraAlgorithm.cs  The algorithm itself — depends only on IGraph
      PathResult.cs         Route + total cost, or "no route exists"
    Configuration/          Turns external network definitions into a graph
      NetworkDefinition.cs  Plain DTOs (nodes + connections)
      NetworkConfigLoader.cs  Reads a NetworkDefinition from JSON
      GraphFactory.cs       NetworkDefinition -> IGraph
  Dijkstra.App/              Console presentation layer
    Program.cs               Loads a network, prompts for start/destination, prints the route
    network.sample.json      Example 6-point network
  Dijkstra.Desktop/          WPF presentation layer — graphical visualizer
    MainWindow.xaml(.cs)     Draws points/connections on a canvas, highlights the optimal route,
                             and exposes inputs to parameterize the network at run time
tests/
  Dijkstra.Tests/            xUnit test suite (30 tests)
```

## Design notes

- **The algorithm never knows how big the network is or how it was built.**
  `DijkstraAlgorithm` only depends on `IGraph`, an interface exposing node ids and
  outgoing edges. Whether the graph has 5 points or 5,000, built from code, from JSON,
  or from a database, the algorithm is unaffected.
- **Network size and structure are external configuration, not code.**
  `NetworkDefinition` + `NetworkConfigLoader` let a network of any size be described in
  a JSON file and turned into a graph via `GraphFactory`, with zero changes to the
  domain model or the algorithm.
- **Separation of concerns** follows the four layers called out in the brief: graph
  representation (`Domain`), route calculation (`Algorithms`), configuration
  (`Configuration`), and presentation — two independent presentation layers
  (`Dijkstra.App` console, `Dijkstra.Desktop` WPF) both sit on top of the same
  `Dijkstra.Core` library without it knowing either exists.
- **Complexity:** `DijkstraAlgorithm` uses a binary-heap `PriorityQueue<string, double>`,
  giving `O((V + E) log V)` time, the standard bound for Dijkstra with a binary heap.
- **Edge cases handled:** no route between two points, start equals destination,
  single-point networks, unknown start/destination points (throws `ArgumentException`),
  and rejection of negative connection weights (Dijkstra's precondition).

## Running the console app

```bash
dotnet run --project src/Dijkstra.App -- src/Dijkstra.App/network.sample.json A F
```

Omit the start/destination arguments to be prompted interactively; omit the config path
to use the bundled `network.sample.json`.

## Running the desktop visualizer

```bash
dotnet run --project src/Dijkstra.Desktop
```

A graphical, standalone sample of the same `Dijkstra.Core` logic (Windows only — WPF).
It draws the network's points and connections on a canvas and highlights the calculated
optimal route (green = start, red = destination, orange = on the route). The network is
fully parameterized from the UI:

- **Generate a random network** — enter a point count and click *Generate* to build a
  random connected network of that size (spanning tree + extra random connections).
- **Build it manually** — add individual points, then connections between them with a
  configurable weight and direction (bidirectional or not).
- **Load sample network** — loads the same 6-point network as the console app.
- Pick a **start** and **destination** from the current points and click
  *Calculate optimal route* to run Dijkstra's algorithm and see the result both as text
  (route + total cost, or "no route exists") and highlighted on the canvas.

## Running the tests

```bash
dotnet test
```

The suite covers: a simple graph with a known optimal route, a graph where the direct
connection is not optimal, multiple possible routes, a graph with no route between
origin and destination, a single-point graph, varying network sizes (5/10/50/100
points), varying connection weights, total-cost correctness, and that the returned
path is actually optimal (cross-checked against a brute-force search).
