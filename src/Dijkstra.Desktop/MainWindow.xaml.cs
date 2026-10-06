using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Dijkstra.Core.Algorithms;
using Dijkstra.Core.Configuration;
using Dijkstra.Core.Domain;

namespace Dijkstra.Desktop;

/// <summary>
/// Aplicación de muestra gráfica. Es una capa de presentación independiente: solo se
/// comunica con los tipos públicos de Dijkstra.Core (<see cref="Graph"/>,
/// <see cref="IShortestPathAlgorithm"/>, <see cref="GraphFactory"/>) y dibuja la red y
/// el resultado que estos produzcan.
/// </summary>
public partial class MainWindow : Window
{
    private const double NodeRadius = 20;

    private readonly IShortestPathAlgorithm _algorithm = new DijkstraAlgorithm();
    private readonly Random _random = new();
    private readonly Dictionary<string, Point> _positions = new();

    private Graph _graph = new();
    private PathResult? _lastResult;

    public MainWindow()
    {
        InitializeComponent();
        RefreshAfterGraphChange();
    }

    private void AddPointButton_Click(object sender, RoutedEventArgs e)
    {
        var id = NewPointIdBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(id))
        {
            ShowResult("Ingresa un id de punto antes de agregarlo.", isError: true);
            return;
        }

        if (_graph.ContainsNode(id))
        {
            ShowResult($"El punto '{id}' ya existe.", isError: true);
            return;
        }

        _graph.AddNode(id);
        NewPointIdBox.Clear();
        ShowResult($"Punto '{id}' agregado.", isError: false);
        RefreshAfterGraphChange();
    }

    private void AddConnectionButton_Click(object sender, RoutedEventArgs e)
    {
        var from = FromCombo.SelectedItem as string;
        var to = ToCombo.SelectedItem as string;

        if (from is null || to is null)
        {
            ShowResult("Selecciona un punto de origen ('Desde') y uno de destino ('Hasta').", isError: true);
            return;
        }

        if (!double.TryParse(WeightBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var weight))
        {
            ShowResult("El peso debe ser un número.", isError: true);
            return;
        }

        try
        {
            _graph.AddEdge(from, to, weight, BidirectionalCheckBox.IsChecked == true);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            ShowResult(ex.Message, isError: true);
            return;
        }

        ShowResult($"Conexión {from} -> {to} (peso {weight}) agregada.", isError: false);
        RefreshAfterGraphChange();
    }

    private void GenerateRandomButton_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(RandomPointCountBox.Text, out var pointCount) || pointCount < 1 || pointCount > 200)
        {
            ShowResult("Ingresa una cantidad de puntos entre 1 y 200.", isError: true);
            return;
        }

        var ids = Enumerable.Range(0, pointCount).Select(i => $"P{i}").ToArray();
        var graph = new Graph();
        foreach (var id in ids)
        {
            graph.AddNode(id);
        }

        // Un árbol de expansión aleatorio garantiza que todos los puntos sean alcanzables, sin importar el tamaño.
        for (var i = 1; i < pointCount; i++)
        {
            var parent = ids[_random.Next(i)];
            graph.AddEdge(parent, ids[i], _random.Next(1, 20), bidirectional: true);
        }

        // Algunas conexiones extra para que normalmente existan varias rutas posibles.
        var extraConnections = pointCount / 2;
        for (var i = 0; i < extraConnections; i++)
        {
            var a = ids[_random.Next(pointCount)];
            var b = ids[_random.Next(pointCount)];
            if (a == b)
            {
                continue;
            }

            graph.AddEdge(a, b, _random.Next(1, 20), bidirectional: true);
        }

        _graph = graph;
        _lastResult = null;
        ShowResult($"Se generó una red aleatoria con {pointCount} punto(s).", isError: false);
        RefreshAfterGraphChange();
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        var start = StartCombo.SelectedItem as string;
        var destination = DestinationCombo.SelectedItem as string;

        if (start is null || destination is null)
        {
            ShowResult("Selecciona un punto de origen y uno de destino.", isError: true);
            return;
        }

        try
        {
            _lastResult = _algorithm.FindShortestPath(_graph, start, destination);
        }
        catch (ArgumentException ex)
        {
            ShowResult(ex.Message, isError: true);
            return;
        }

        if (_lastResult.RouteFound)
        {
            ShowResult(
                $"Ruta óptima: {string.Join(" -> ", _lastResult.Path)}\nCosto total: {_lastResult.TotalCost}",
                isError: false);
        }
        else
        {
            ShowResult($"No existe ninguna ruta entre '{start}' y '{destination}'.", isError: true);
        }

        Redraw();
    }

    private void LoadSampleButton_Click(object sender, RoutedEventArgs e)
    {
        var definition = new NetworkDefinition
        {
            Nodes = new List<string> { "A", "B", "C", "D", "E", "F" },
            Connections = new List<ConnectionDefinition>
            {
                new() { From = "A", To = "B", Weight = 4, Bidirectional = true },
                new() { From = "A", To = "C", Weight = 2, Bidirectional = true },
                new() { From = "C", To = "B", Weight = 1, Bidirectional = true },
                new() { From = "B", To = "D", Weight = 5, Bidirectional = true },
                new() { From = "C", To = "D", Weight = 8, Bidirectional = true },
                new() { From = "C", To = "E", Weight = 10, Bidirectional = true },
                new() { From = "D", To = "E", Weight = 2, Bidirectional = true },
                new() { From = "D", To = "F", Weight = 6, Bidirectional = true },
                new() { From = "E", To = "F", Weight = 3, Bidirectional = true },
            },
        };

        _graph = (Graph)GraphFactory.CreateFromDefinition(definition);
        _lastResult = null;
        ShowResult("Se cargó la red de ejemplo (puntos A-F).", isError: false);
        RefreshAfterGraphChange();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        _graph = new Graph();
        _lastResult = null;
        ShowResult("Red limpiada.", isError: false);
        RefreshAfterGraphChange();
    }

    private void NetworkCanvas_SizeChanged(object sender, SizeChangedEventArgs e) => Redraw();

    private void RefreshAfterGraphChange()
    {
        var nodeIds = _graph.NodeIds.OrderBy(id => id, StringComparer.OrdinalIgnoreCase).ToList();

        UpdateCombo(FromCombo, nodeIds);
        UpdateCombo(ToCombo, nodeIds);
        UpdateCombo(StartCombo, nodeIds);
        UpdateCombo(DestinationCombo, nodeIds);

        Redraw();
    }

    private static void UpdateCombo(ComboBox combo, List<string> nodeIds)
    {
        var previouslySelected = combo.SelectedItem as string;
        combo.ItemsSource = nodeIds;
        combo.SelectedItem = previouslySelected is not null && nodeIds.Contains(previouslySelected)
            ? previouslySelected
            : nodeIds.FirstOrDefault();
    }

    private void ShowResult(string message, bool isError)
    {
        ResultText.Text = message;
        ResultText.Foreground = isError ? Brushes.Firebrick : Brushes.DarkGreen;
    }

    private void Redraw()
    {
        NetworkCanvas.Children.Clear();

        var nodeIds = _graph.NodeIds.OrderBy(id => id, StringComparer.OrdinalIgnoreCase).ToList();
        if (nodeIds.Count == 0)
        {
            return;
        }

        ComputeCircularLayout(nodeIds);

        var pathPairs = BuildPathPairs();
        var allEdges = nodeIds.SelectMany(id => _graph.GetOutgoingEdges(id)).ToList();
        var drawnPairs = new HashSet<(string, string)>();

        foreach (var edge in allEdges)
        {
            var reverseExists = allEdges.Any(other => other.From == edge.To && other.To == edge.From);
            var key = NormalizedPair(edge.From, edge.To);

            if (reverseExists)
            {
                if (!drawnPairs.Add(key))
                {
                    continue;
                }

                DrawEdge(edge.From, edge.To, edge.Weight, directed: false, IsPathEdge(pathPairs, edge.From, edge.To));
            }
            else
            {
                DrawEdge(edge.From, edge.To, edge.Weight, directed: true, IsPathEdge(pathPairs, edge.From, edge.To));
            }
        }

        var start = _lastResult is { RouteFound: true } r ? r.Path[0] : null;
        var destination = _lastResult is { RouteFound: true } r2 ? r2.Path[^1] : null;

        foreach (var nodeId in nodeIds)
        {
            var isOnPath = _lastResult is { RouteFound: true } result && result.Path.Contains(nodeId);
            DrawNode(nodeId, isOnPath, nodeId == start, nodeId == destination);
        }
    }

    private void ComputeCircularLayout(List<string> nodeIds)
    {
        _positions.Clear();

        var width = NetworkCanvas.ActualWidth > 0 ? NetworkCanvas.ActualWidth : 760;
        var height = NetworkCanvas.ActualHeight > 0 ? NetworkCanvas.ActualHeight : 560;
        var centerX = width / 2;
        var centerY = height / 2;
        var radius = Math.Max(60, Math.Min(width, height) / 2 - 60);

        if (nodeIds.Count == 1)
        {
            _positions[nodeIds[0]] = new Point(centerX, centerY);
            return;
        }

        for (var i = 0; i < nodeIds.Count; i++)
        {
            var angle = 2 * Math.PI * i / nodeIds.Count;
            var x = centerX + radius * Math.Cos(angle);
            var y = centerY + radius * Math.Sin(angle);
            _positions[nodeIds[i]] = new Point(x, y);
        }
    }

    private void DrawNode(string nodeId, bool isOnPath, bool isStart, bool isDestination)
    {
        var point = _positions[nodeId];

        var fill = isStart ? Brushes.SeaGreen
            : isDestination ? Brushes.IndianRed
            : isOnPath ? Brushes.Orange
            : Brushes.SteelBlue;

        var ellipse = new Ellipse
        {
            Width = NodeRadius * 2,
            Height = NodeRadius * 2,
            Fill = fill,
            Stroke = Brushes.Black,
            StrokeThickness = 1,
        };
        Canvas.SetLeft(ellipse, point.X - NodeRadius);
        Canvas.SetTop(ellipse, point.Y - NodeRadius);
        Canvas.SetZIndex(ellipse, 1);
        NetworkCanvas.Children.Add(ellipse);

        var label = new TextBlock
        {
            Text = nodeId,
            Foreground = Brushes.White,
            FontWeight = FontWeights.Bold,
            FontSize = 12,
        };
        label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        Canvas.SetLeft(label, point.X - label.DesiredSize.Width / 2);
        Canvas.SetTop(label, point.Y - label.DesiredSize.Height / 2);
        Canvas.SetZIndex(label, 2);
        NetworkCanvas.Children.Add(label);
    }

    private void DrawEdge(string fromId, string toId, double weight, bool directed, bool isPathEdge)
    {
        var from = _positions[fromId];
        var to = _positions[toId];

        var brush = isPathEdge ? Brushes.OrangeRed : Brushes.LightGray;
        var thickness = isPathEdge ? 3.5 : 1.5;

        var line = new Line { X1 = from.X, Y1 = from.Y, X2 = to.X, Y2 = to.Y, Stroke = brush, StrokeThickness = thickness };
        Canvas.SetZIndex(line, 0);
        NetworkCanvas.Children.Add(line);

        if (directed)
        {
            DrawArrowHead(from, to, brush);
        }

        var midX = (from.X + to.X) / 2;
        var midY = (from.Y + to.Y) / 2;
        var weightLabel = new TextBlock
        {
            Text = weight.ToString(CultureInfo.InvariantCulture),
            Background = Brushes.White,
            Foreground = isPathEdge ? Brushes.OrangeRed : Brushes.DimGray,
            FontSize = 11,
            Padding = new Thickness(2, 0, 2, 0),
        };
        Canvas.SetLeft(weightLabel, midX);
        Canvas.SetTop(weightLabel, midY);
        Canvas.SetZIndex(weightLabel, 2);
        NetworkCanvas.Children.Add(weightLabel);
    }

    private void DrawArrowHead(Point from, Point to, Brush brush)
    {
        const double arrowLength = 10;
        const double arrowWidth = 7;

        var dx = to.X - from.X;
        var dy = to.Y - from.Y;
        var length = Math.Sqrt(dx * dx + dy * dy);
        if (length < 1e-6)
        {
            return;
        }

        var ux = dx / length;
        var uy = dy / length;

        // La punta de la flecha termina en el borde del nodo destino, no en su centro.
        var tip = new Point(to.X - ux * NodeRadius, to.Y - uy * NodeRadius);
        var baseCenter = new Point(tip.X - ux * arrowLength, tip.Y - uy * arrowLength);
        var perpX = -uy;
        var perpY = ux;

        var p1 = new Point(baseCenter.X + perpX * arrowWidth / 2, baseCenter.Y + perpY * arrowWidth / 2);
        var p2 = new Point(baseCenter.X - perpX * arrowWidth / 2, baseCenter.Y - perpY * arrowWidth / 2);

        var arrow = new Polygon { Points = new PointCollection { tip, p1, p2 }, Fill = brush };
        Canvas.SetZIndex(arrow, 1);
        NetworkCanvas.Children.Add(arrow);
    }

    private HashSet<(string, string)> BuildPathPairs()
    {
        var pairs = new HashSet<(string, string)>();
        if (_lastResult is { RouteFound: true } result)
        {
            for (var i = 0; i < result.Path.Count - 1; i++)
            {
                pairs.Add(NormalizedPair(result.Path[i], result.Path[i + 1]));
            }
        }

        return pairs;
    }

    private static bool IsPathEdge(HashSet<(string, string)> pathPairs, string a, string b) =>
        pathPairs.Contains(NormalizedPair(a, b));

    private static (string, string) NormalizedPair(string a, string b) =>
        string.CompareOrdinal(a, b) <= 0 ? (a, b) : (b, a);
}
