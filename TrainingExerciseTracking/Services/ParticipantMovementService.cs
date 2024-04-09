using System.Collections.Concurrent;
using Prism.Events;
using TrainingExerciseTracking.Database.Models;

namespace TrainingExerciseTracking.Services;

public delegate void BatchCollected(List<Movement> movements);

public class ParticipantMovementService : IParticipantMovementService
{
    private object _triggerEventLock = new();
    private DateTime _lastBatch = DateTime.Now;

    private ConcurrentQueue<Movement> _movements = new ConcurrentQueue<Movement>();
    
    public event BatchCollected OnBatchCollected;
    
    public ParticipantMovementService(IEventAggregator eventAggregator)
    {
        eventAggregator.GetEvent<ParticipantMovementEvent>().Subscribe(CollectMovement);
    }

    private void CollectMovement(Movement movement)
    {
        _movements.Enqueue(movement);
        if (DateTime.Now - _lastBatch <= TimeSpan.FromMilliseconds(500)) return;
        lock (_triggerEventLock)
        {
            // doing a sound check for requests that got queued up
            if (DateTime.Now - _lastBatch <= TimeSpan.FromMilliseconds(500)) return;
            var currentMovements = new List<Movement>();
            var i = 0;
            while (!_movements.IsEmpty && i < 100)
            {
                Movement item;
                if (_movements.TryDequeue(out var mov))
                {
                    currentMovements.Add(mov);
                }
                i++;
            }

            if (currentMovements.Count > 0) OnBatchCollected?.Invoke(currentMovements);
            _lastBatch = DateTime.Now;
        }
    }
}