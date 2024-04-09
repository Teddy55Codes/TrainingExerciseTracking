using System.Collections.Concurrent;
using Prism.Events;
using TrainingExerciseTracking.Database.Models;

namespace TrainingExerciseTracking.Services;

public delegate void BatchCollected(Movement[] movements);

public class ParticipantMovementService : IParticipantMovementService
{
    private DateTime _lastBatch = DateTime.Now;
    
    public ConcurrentBag<Movement> Movements { get; }
    
    public event BatchCollected OnBatchCollected;
    
    public ParticipantMovementService(IEventAggregator eventAggregator)
    {
        Movements = new ConcurrentBag<Movement>();
        eventAggregator.GetEvent<ParticipantMovementEvent>().Subscribe(CollectMovement);
    }

    private void CollectMovement(Movement movement)
    {
        Movements.Add(movement);
        if (DateTime.Now - _lastBatch > TimeSpan.FromMilliseconds(500))
        {
            OnBatchCollected?.Invoke(Movements.ToArray());
            _lastBatch = DateTime.Now;
        }
    }
}