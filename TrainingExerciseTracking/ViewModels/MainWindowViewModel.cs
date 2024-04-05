using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Tiling;
using Mapsui.UI.Wpf;
using Prism.Events;
using TestingTool.Ui;
using TrainingExerciseTracking.Database.Models;
using TrainingExerciseTracking.Services;

namespace TrainingExerciseTracking.ViewModels;

public class MainWindowViewModel : BaseViewModel
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IParticipantMovementService _participantMovementService;
    private readonly MapControl _mapControl;
    
    private WritableLayer _iconLayer;
    private List<ExtendedPoint> _extendedPoints = new();
    private Mapsui.Widgets.TextBox _textBox = new();

    
    public MainWindowViewModel(IEventAggregator eventAggregator, IParticipantMovementService participantMovementService, MapControl mapControl)
    {
        _eventAggregator = eventAggregator;
        _participantMovementService = participantMovementService;
        _mapControl = mapControl;
        InitializeMap();
        _participantMovementService.OnBatchCollected += UpdateParticipants;
    }

    private void UpdateParticipants(Movement[] movements)
    {
        var currentMovements = new List<Movement>();
        var i = 0;
        while (!_participantMovementService.Movements.IsEmpty && i < 500)
        {
            Task.Run(() =>
            {
                Movement item;
                if (_participantMovementService.Movements.TryTake(out var movement))
                {
                    currentMovements.Add(movement);
                }
            });
            i++;
        }
        if (currentMovements.Count == 0) return;
        
        for (int j = 0; j < currentMovements.Count; j++)
        {
            var curr = _extendedPoints.SingleOrDefault(exp => exp.Id == currentMovements[j].Participant.Number);
            if (curr == null)
            {
                _extendedPoints.Add(new ExtendedPoint()
                {
                    Id = currentMovements[j].Participant.Number, 
                    Point = new MPoint(currentMovements[j].Longitude, currentMovements[j].Latitude)
                });
            }
            else
            {
                curr.Point.X = currentMovements[j].Longitude;
                curr.Point.Y = currentMovements[j].Latitude;
            }
        }

        UpdateIconLocation();
    }
    
    public void MyMapControl_Info(object? sender, MapInfoEventArgs e)
    {
        var radius = e.MapInfo.WorldPosition.MRect.Centroid.MRect.Grow(e.MapInfo.Resolution*10);
        foreach (var exp in _extendedPoints)
        {
            if (radius.Contains(exp.Point))
            {
                _textBox.Text = $"Participant clicked: {exp.Id}";
                _mapControl.Refresh();
                return;
            }
        }
    }
    
    private void InitializeMap()
    {
        _mapControl.Map = new Map
        {
            CRS = "EPSG:3857",
        };
        _mapControl.Map.Layers.Add(OpenStreetMap.CreateTileLayer());
        _textBox.HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Left;
        _textBox.VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Top;
        _mapControl.Map.Widgets.Add(_textBox);
        
        // Initialize and add the icon layer
        _iconLayer = new WritableLayer();
        _mapControl.Map.Layers.Add(_iconLayer);

        // Initially add an icon
        UpdateIconLocation();
    }
    
    private void UpdateIconLocation()
    {
        _iconLayer.Clear();
        foreach (var extendedPoint in _extendedPoints)
        {
            var sphericalMercatorCoordinate = SphericalMercator.FromLonLat(extendedPoint.Point.X, extendedPoint.Point.Y);
            _iconLayer.Add(new PointFeature(sphericalMercatorCoordinate.x, sphericalMercatorCoordinate.y));
        }
        _mapControl.Refresh();
    }
}