using System.Windows;
using Prism.Events;
using TrainingExerciseTracking.Database.Models;

namespace TrainingExerciseTracking.Services;

public class ParticipantActivityGenerator : IParticipantActivityGenerator
{
    private readonly IEventAggregator _eventAggregator;
    private readonly Random _random;
    private TimeSpan publishDelay = TimeSpan.FromMilliseconds(500);
    private int _participants = 10;
    private string[] _countries = ["Germany", "France"];
    private string[] _ranks = ["Normal", "Elevated"];
    private double longitudeStart = 7.27173;
    private double longitudeEnd = 7.37173;
    private double latitudeStart = 46.56528;
    private double latitudeEnd = 46.66528;
    
    public ParticipantActivityGenerator(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        _random = new Random();
    }

    public void Start()
    {
        while (true)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var participantNumber = _random.Next(1, _participants + 1);
                _eventAggregator.GetEvent<ParticipantMovementEvent>().Publish(
                    new Movement()
                    {
                        Latitude = _random.NextDouble() * (latitudeEnd - latitudeStart) + latitudeStart,
                        Longitude = _random.NextDouble() * (longitudeEnd - longitudeStart) + longitudeStart,
                        Participant = new Participant()
                        {
                            Number = participantNumber,
                            Country = _countries[_random.Next(_countries.Length)],
                            Rank = _ranks[_random.Next(_ranks.Length)],
                            Information = string.Empty
                        }
                    }
                );
            });
            Thread.Sleep(publishDelay);
        }
    }
}