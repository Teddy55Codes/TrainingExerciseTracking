using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Tiling;
using Mapsui.UI.Wpf;
using Mapsui.Widgets;
using Prism.Events;
using TestingTool.Ui;
using TrainingExerciseTracking.Database;
using TrainingExerciseTracking.Database.Models;
using TrainingExerciseTracking.Services;

namespace TrainingExerciseTracking.ViewModels;

public class MainWindowViewModel : BaseViewModel
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IParticipantMovementService _participantMovementService;
    private readonly MapControl _mapControl;

    private object _lockPoints = new();
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

    private void UpdateParticipants(List<Movement> movements)
    {
        lock (_lockPoints)
        {
            for (int j = 0; j < movements.Count; j++)
            {
                var curr = _extendedPoints.SingleOrDefault(exp => exp.ParticipantNumber == movements[j].Participant.Number);
                if (curr == null)
                {
                    _extendedPoints.Add(new ExtendedPoint()
                    {
                        ParticipantNumber = movements[j].Participant.Number, 
                        Point = new MPoint(movements[j].Longitude, movements[j].Latitude)
                    });
                }
                else
                {
                    curr.Point.X = movements[j].Longitude;
                    curr.Point.Y = movements[j].Latitude;
                }
            }
        }

        UpdateIconLocation();
    }
    
    public void MyMapControl_Info(object? sender, MapInfoEventArgs e)
    {
        var radius = e.MapInfo.WorldPosition.MRect.Centroid.MRect.Grow(e.MapInfo.Resolution*10);
        foreach (var exp in _extendedPoints)
        {
            var expRadius = SphericalMercator.FromLonLat(exp.Point.X, exp.Point.Y);
            if (radius.Contains(new MPoint(expRadius.x, expRadius.y)))  
            {
                using (var db = new TrainingDbContext())
                {
                    var participant = db.Participants.First(p => p.Number == exp.ParticipantNumber);
                    
                    _textBox.Text = $"Number: {participant.Number} | Country: {participant.Country} | Rank: {participant.Rank} | Longitude: {exp.Point.X} | Latitude: {exp.Point.Y} | Information: {participant.Information}";
                }
                
                _mapControl.Refresh();
                return;
            }
        }
        _textBox.Text = String.Empty;
    }
    
    private void InitializeMap()
    {
        _mapControl.Map = new Map
        {
            CRS = "EPSG:3857",
        };
        _mapControl.Map.Layers.Add(OpenStreetMap.CreateTileLayer());
        
        _textBox.HorizontalAlignment = HorizontalAlignment.Left;
        _textBox.VerticalAlignment = VerticalAlignment.Top;
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